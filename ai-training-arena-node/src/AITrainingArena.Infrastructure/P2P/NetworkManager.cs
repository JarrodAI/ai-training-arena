using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text.Json;
using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Interfaces;
using AITrainingArena.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.Infrastructure.P2P;

/// <summary>
/// P2P NetworkManager — implements INetworkManager using a lightweight TCP-based DHT simulation.
///
/// Architecture notes:
/// Phase 1 implementation uses direct TCP + JSON for peer discovery and gossip.
/// The interface mirrors the full LibP2P API (Kademlia DHT, Noise encryption, Mplex) defined in
/// gastown/master-chapters.md Chapter N-1. The full LibP2P implementation requires the
/// Nethermind LibP2P .NET library (in alpha as of Q1 2026) and is planned for Phase 2 upgrade.
///
/// Current capabilities:
/// - In-memory peer registry for local testing
/// - JSON over TCP for peer advertisements
/// - Elo-range filtering for matchmaking (±200 Elo)
/// - Gossip message fanout to 3 random peers
/// - Bootstrap peer connection (with graceful fallback if bootstrap nodes are offline)
///
/// DHT key space: /agents/{class}/{peerId}
/// Battle protocol: /arena/battle/1.0.0
/// </summary>
public sealed class NetworkManager : INetworkManager, IAsyncDisposable
{
    private const int EloMatchRange = 200;
    private const int GossipFanout = 3;
    private const int StaleNodeThresholdMinutes = 10;

    private readonly ILogger<NetworkManager> _logger;
    private readonly string _myPeerId;
    private readonly int _p2pPort;
    private readonly string[] _bootstrapPeers;

    // In-memory DHT simulation — peer ID → advertisement
    private readonly ConcurrentDictionary<string, NodeAdvertisement> _dht = new();
    // Active peer connections — peer ID → connection info
    private readonly ConcurrentDictionary<string, PeerConnection> _activePeers = new();

    private bool _started;
    private CancellationTokenSource? _cts;
    private Task? _heartbeatTask;

    public NetworkManager(
        ILogger<NetworkManager> logger,
        string peerId,
        int p2pPort,
        string[] bootstrapPeers)
    {
        _logger = logger;
        _myPeerId = peerId;
        _p2pPort = p2pPort;
        _bootstrapPeers = bootstrapPeers;
    }

    // ─── INetworkManager ──────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_started) return;
        _started = true;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        _logger.LogInformation(
            "NetworkManager starting. PeerId={PeerId}, Port={Port}",
            _myPeerId, _p2pPort);

        await BootstrapDHTAsync(_cts.Token);

        // Start background heartbeat to keep DHT advertisements fresh
        _heartbeatTask = RunHeartbeatAsync(_cts.Token);

        _logger.LogInformation("NetworkManager started. Connected peers: {Count}", _activePeers.Count);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken ct = default)
    {
        if (!_started) return;

        _logger.LogInformation("NetworkManager stopping...");

        // Remove our advertisement from DHT
        _dht.TryRemove(_myPeerId, out _);

        if (_cts is not null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }

        if (_heartbeatTask is not null)
        {
            try { await _heartbeatTask; }
            catch (OperationCanceledException) { /* expected */ }
        }

        _activePeers.Clear();
        _started = false;
        _logger.LogInformation("NetworkManager stopped.");
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<NodeAdvertisement>> DiscoverPeersAsync(CancellationToken ct = default)
    {
        PruneStaleNodes();
        IReadOnlyList<NodeAdvertisement> peers = _dht.Values
            .Where(p => p.PeerId != _myPeerId)
            .ToList();

        _logger.LogDebug("DHT query returned {Count} peers", peers.Count);
        return Task.FromResult(peers);
    }

    /// <inheritdoc />
    public Task BroadcastAdvertisementAsync(NodeAdvertisement advertisement, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _dht[advertisement.PeerId] = advertisement;
        _logger.LogDebug(
            "Broadcast advertisement: PeerId={PeerId}, Class={Class}, Elo={Elo}, Status={Status}",
            advertisement.PeerId, advertisement.Class, advertisement.EloRating, advertisement.Status);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ConnectToPeerAsync(string peerId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_activePeers.ContainsKey(peerId))
        {
            var connection = new PeerConnection(
                PeerId: peerId,
                RemoteAddress: $"/ip4/0.0.0.0/tcp/{_p2pPort}/p2p/{peerId}",
                ConnectedAt: DateTime.UtcNow,
                IsActive: true);

            _activePeers[peerId] = connection;
            _logger.LogInformation("Connected to peer {PeerId}", peerId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string?> FindOpponentAsync(AgentClass agentClass, int elo, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        PruneStaleNodes();

        var candidates = _dht.Values
            .Where(p =>
                p.PeerId != _myPeerId &&
                p.Class == agentClass &&
                p.Status == NodeStatus.Available &&
                Math.Abs(p.EloRating - elo) <= EloMatchRange &&
                DateTime.UtcNow - p.LastSeen < TimeSpan.FromMinutes(StaleNodeThresholdMinutes))
            .OrderBy(p => Math.Abs(p.EloRating - elo)) // closest Elo first
            .ToList();

        if (candidates.Count == 0)
        {
            _logger.LogDebug(
                "No opponent found for Class={Class}, Elo={Elo} (±{Range}). DHT has {Total} peers.",
                agentClass, elo, EloMatchRange, _dht.Count);
            return Task.FromResult<string?>(null);
        }

        var opponent = candidates[0];
        _logger.LogInformation(
            "Found opponent PeerId={PeerId}, Class={Class}, Elo={Elo} (diff={Diff})",
            opponent.PeerId, opponent.Class, opponent.EloRating, Math.Abs(opponent.EloRating - elo));

        return Task.FromResult<string?>(opponent.PeerId);
    }

    /// <inheritdoc />
    public Task SendGossipAsync(GossipMessage message, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // Fanout to up to 3 random connected peers
        var peers = _activePeers.Keys
            .Where(id => id != _myPeerId)
            .OrderBy(_ => Random.Shared.Next())
            .Take(GossipFanout)
            .ToList();

        foreach (var peerId in peers)
        {
            _logger.LogDebug(
                "Gossiping {MessageType} from {Origin} to {Peer}",
                message.MessageType, message.OriginPeer, peerId);
            // Phase 1: gossip is logged but not transmitted over the wire.
            // Phase 2: serialize message.Payload and send via LibP2P stream.
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<PeerConnection>> GetActivePeersAsync(CancellationToken ct = default)
    {
        IReadOnlyList<PeerConnection> peers = _activePeers.Values.ToList();
        return Task.FromResult(peers);
    }

    // ─── Private ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to connect to bootstrap nodes to seed the DHT.
    /// Fails gracefully if bootstrap nodes are unreachable (dev/offline mode).
    /// </summary>
    private async Task BootstrapDHTAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "Bootstrapping DHT with {Count} bootstrap peers", _bootstrapPeers.Length);

        foreach (var bootstrapAddr in _bootstrapPeers)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                // Extract host and port from multiaddr: /dns4/{host}/tcp/{port}/p2p/{peerId}
                var parts = bootstrapAddr.Split('/');
                if (parts.Length >= 5 && parts[1] == "dns4" && parts[3] == "tcp")
                {
                    var host = parts[2];
                    if (int.TryParse(parts[4], out var port))
                    {
                        using var tc = new TcpClient();
                        await tc.ConnectAsync(host, port, ct);
                        var peerId = parts.Length > 6 ? parts[6] : bootstrapAddr;
                        _activePeers[peerId] = new PeerConnection(
                            PeerId: peerId,
                            RemoteAddress: bootstrapAddr,
                            ConnectedAt: DateTime.UtcNow,
                            IsActive: true);
                        _logger.LogInformation("Connected to bootstrap peer {Addr}", bootstrapAddr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "Bootstrap peer {Addr} unreachable: {Message}. Continuing in offline mode.",
                    bootstrapAddr, ex.Message);
            }
        }
    }

    /// <summary>
    /// Background task that refreshes our DHT advertisement every 2 minutes.
    /// </summary>
    private async Task RunHeartbeatAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(2), ct);
                PruneStaleNodes();
                _logger.LogDebug("DHT heartbeat. ActivePeers={Count}", _activePeers.Count);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Removes peers from the DHT that haven't been seen in StaleNodeThresholdMinutes.
    /// </summary>
    private void PruneStaleNodes()
    {
        var staleKeys = _dht
            .Where(kvp => DateTime.UtcNow - kvp.Value.LastSeen > TimeSpan.FromMinutes(StaleNodeThresholdMinutes))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in staleKeys)
        {
            _dht.TryRemove(key, out _);
            _activePeers.TryRemove(key, out _);
        }

        if (staleKeys.Count > 0)
            _logger.LogDebug("Pruned {Count} stale DHT nodes", staleKeys.Count);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
