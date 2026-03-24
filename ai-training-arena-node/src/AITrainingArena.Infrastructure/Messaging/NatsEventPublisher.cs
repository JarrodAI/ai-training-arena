using AITrainingArena.Domain.Interfaces;
using AITrainingArena.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using System.Text.Json;

namespace AITrainingArena.Infrastructure.Messaging;

/// <summary>
/// NATS JetStream event publisher for inter-service messaging.
/// Publishes domain events to NATS subjects for cross-node synchronization.
/// Connection: nats://localhost:4222 (default). Graceful fallback if unavailable.
/// </summary>
public sealed class NatsEventPublisher : IAsyncDisposable
{
    private const string BattleCompletedSubject = "arena.battle.completed";
    private const string EloUpdatedSubject = "arena.elo.updated";
    private const string LeaderboardSyncSubject = "arena.leaderboard.sync";

    private readonly ILogger<NatsEventPublisher> _logger;
    private readonly string _natsUrl;
    private NatsConnection? _connection;
    private bool _connected;

    public NatsEventPublisher(string natsUrl, ILogger<NatsEventPublisher> logger)
    {
        _natsUrl = natsUrl;
        _logger = logger;
    }

    /// <summary>Establishes NATS JetStream connection. Fails gracefully if unavailable.</summary>
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            var opts = new NatsOpts { Url = _natsUrl };
            _connection = new NatsConnection(opts);
            await _connection.ConnectAsync();
            _connected = true;
            _logger.LogInformation("Connected to NATS at {Url}", _natsUrl);
        }
        catch (Exception ex)
        {
            _connected = false;
            _logger.LogWarning(ex, "NATS unavailable at {Url}. Events will be dropped.", _natsUrl);
        }
    }

    /// <summary>Publishes a battle completed event to NATS JetStream.</summary>
    public async Task PublishBattleCompletedAsync(BattleResult result, Guid battleId, CancellationToken ct = default)
    {
        if (!_connected || _connection is null) return;
        var payload = JsonSerializer.SerializeToUtf8Bytes(new
        {
            battleId = battleId.ToString(),
            winner = result.Winner,
            proposerScore = result.ProposerScore,
            solverScore = result.SolverScore,
            timestamp = DateTime.UtcNow,
        });
        await PublishAsync(BattleCompletedSubject, payload, ct);
    }

    /// <summary>Publishes an Elo update event to NATS JetStream.</summary>
    public async Task PublishEloUpdatedAsync(uint nftId, int oldElo, int newElo, CancellationToken ct = default)
    {
        if (!_connected || _connection is null) return;
        var payload = JsonSerializer.SerializeToUtf8Bytes(new
        {
            nftId,
            oldElo,
            newElo,
            timestamp = DateTime.UtcNow,
        });
        await PublishAsync(EloUpdatedSubject, payload, ct);
    }

    /// <summary>Publishes a leaderboard sync gossip to NATS.</summary>
    public async Task PublishLeaderboardSyncAsync(object leaderboardData, CancellationToken ct = default)
    {
        if (!_connected || _connection is null) return;
        var payload = JsonSerializer.SerializeToUtf8Bytes(leaderboardData);
        await PublishAsync(LeaderboardSyncSubject, payload, ct);
    }

    private async Task PublishAsync(string subject, byte[] payload, CancellationToken ct)
    {
        try
        {
            await _connection!.PublishAsync(subject, payload, cancellationToken: ct);
            _logger.LogDebug("Published {Bytes} bytes to NATS subject: {Subject}", payload.Length, subject);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish to NATS subject: {Subject}", subject);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
