using AITrainingArena.Domain.Enums;

namespace AITrainingArena.Domain.ValueObjects;

public record LeaderboardEntry(
    uint NftId,
    string ModelName,
    AgentClass Class,
    int EloRating,
    int TotalBattles,
    decimal WinRate,
    decimal TotalRewardsEarned);
