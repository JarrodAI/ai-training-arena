using AITrainingArena.Domain.Enums;

namespace AITrainingArena.Application.DTOs;

public record BattleSummaryDto(
    Guid BattleId,
    string OpponentPeerId,
    BattleRole Role,
    string Winner,
    decimal MyScore,
    decimal OpponentScore,
    DateTime CompletedAt);
