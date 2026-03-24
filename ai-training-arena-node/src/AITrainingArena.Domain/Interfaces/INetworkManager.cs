using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Port for P2P networking operations via LibP2P.
/// </summary>
public interface INetworkManager
{
    /// <summary>Starts the P2P host and bootstraps DHT.</summary>
    /// <param name="ct">Cancellation token.</param>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>Gracefully shuts down the P2P host.</summary>
    /// <param name="ct">Cancellation token.</param>
    Task StopAsync(CancellationToken ct = default);

    /// <summary>Discovers available peers on the network.</summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<NodeAdvertisement>> DiscoverPeersAsync(CancellationToken ct = default);

    /// <summary>Broadcasts this node's availability advertisement to DHT.</summary>
    /// <param name="advertisement">The node advertisement to broadcast.</param>
    /// <param name="ct">Cancellation token.</param>
    Task BroadcastAdvertisementAsync(NodeAdvertisement advertisement, CancellationToken ct = default);

    /// <summary>Connects to a specific peer by ID.</summary>
    /// <param name="peerId">The LibP2P peer identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ConnectToPeerAsync(string peerId, CancellationToken ct = default);

    /// <summary>Finds a suitable opponent within +/-200 Elo range.</summary>
    /// <param name="agentClass">Agent class for matchmaking.</param>
    /// <param name="elo">Current Elo rating.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<string?> FindOpponentAsync(AgentClass agentClass, int elo, CancellationToken ct = default);

    /// <summary>Sends a gossip message to connected peers.</summary>
    /// <param name="message">The gossip message to send.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendGossipAsync(GossipMessage message, CancellationToken ct = default);

    /// <summary>Gets all currently active peer connections.</summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<PeerConnection>> GetActivePeersAsync(CancellationToken ct = default);
}
