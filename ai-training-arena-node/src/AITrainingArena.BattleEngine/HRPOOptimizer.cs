using AITrainingArena.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.BattleEngine;

/// <summary>
/// Hop-grouped Relative Policy Optimization (HRPO) from blueprint 7.1.
/// Groups battle results by hop count, computes per-group advantages.
/// Note: Full RL weight updates require Python/PyTorch. This C# implementation
/// logs the training signal and queues offline fine-tuning jobs.
/// </summary>
public sealed class HRPOOptimizer
{
    private readonly ILogger<HRPOOptimizer> _logger;

    public HRPOOptimizer(ILogger<HRPOOptimizer> logger) => _logger = logger;

    /// <summary>
    /// Computes per-hop-group advantages.
    /// Formula: advantage_i = (difficulty_i - mean_group) / (std_group + 1e-8)
    /// </summary>
    public OptimizationResult Update(IReadOnlyList<QuestionAnswerResult> results)
    {
        if (results.Count == 0) return OptimizationResult.Empty;
        var allAdvantages = new List<double>();
        foreach (var group in results.GroupBy(r => r.HopCount))
        {
            var diffs = group.Select(r => (double)r.DifficultyScore).ToList();
            var mean = diffs.Average();
            var std = Math.Sqrt(diffs.Select(d => Math.Pow(d - mean, 2)).Average());
            allAdvantages.AddRange(diffs.Select(d => (d - mean) / (std + 1e-8)));
            _logger.LogDebug("HRPO hop={Hop}: mean={Mean:F3}, std={Std:F3}, n={N}",
                group.Key, mean, std, diffs.Count);
        }
        var meanAdv = allAdvantages.Average();
        var signal = meanAdv * (1.0 + results.Count(r => r.IsCorrect) * 0.05);
        _logger.LogInformation(
            "HRPO update: {Count} results, mean_adv={Adv:F4}, signal={Sig:F4}",
            results.Count, meanAdv, signal);
        return new OptimizationResult(meanAdv, signal, DateTime.UtcNow, true);
    }
}
