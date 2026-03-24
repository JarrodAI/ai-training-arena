namespace AITrainingArena.BattleEngine;

/// <summary>
/// Result of an HRPO or GRPO optimization step.
/// Contains the training signal for offline model fine-tuning.
/// </summary>
public record OptimizationResult(
    double MeanAdvantage,
    double TrainingSignal,
    DateTime UpdatedAt,
    bool IsQueuedForFineTuning)
{
    public static readonly OptimizationResult Empty =
        new(0.0, 0.0, DateTime.UtcNow, false);
}
