using AITrainingArena.Domain.Enums;

namespace AITrainingArena.Application.DTOs;

public record NodeStatusDto(
    string PeerId,
    NodeStatus Status,
    int ConnectedPeers,
    int CompletedBattles,
    int EloRating,
    bool IsAutomatic);
