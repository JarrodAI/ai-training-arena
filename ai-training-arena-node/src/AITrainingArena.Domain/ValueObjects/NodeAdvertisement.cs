using AITrainingArena.Domain.Enums;

namespace AITrainingArena.Domain.ValueObjects;

/// <summary>
/// DHT advertisement broadcast by a node to announce its availability for matchmaking.
/// Published to DHT key /agents/{class}/{peerId}.
/// </summary>
/// <param name="PeerId">LibP2P peer identifier.</param>
/// <param name="Class">Agent class tier for matchmaking.</param>
/// <param name="EloRating">Current Elo rating for opponent filtering (+/-200 range).</param>
/// <param name="Status">Current node availability status.</param>
/// <param name="LastSeen">Timestamp of last heartbeat or status update.</param>
public record NodeAdvertisement(
    string PeerId,
    AgentClass Class,
    int EloRating,
    NodeStatus Status,
    DateTime LastSeen);
