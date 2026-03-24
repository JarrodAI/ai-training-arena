namespace AITrainingArena.Domain.ValueObjects;

public record BattleHandshake(
    uint MyNftId,
    byte[] MySignature,
    DateTime Timestamp);
