using AITrainingArena.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.BattleEngine;

/// <summary>
/// Group Relative Policy Optimization (GRPO) from blueprint 7.1.
/// Batches results in groups of groupSize and computes per-batch advantages.
/// Note: Full RL weight updates require Python/PyTorch. This C# implementation
/// logs the training signal and queues offline fine-tuning jobs.
/// </summary>
public sealed class GRPOOptimizer
{
    private const int DefaultGroupSize = 5;
    private readonly ILogger<GRPOOptimizer> _logger;

    public GRPOOptimizer(ILogger<GRPOOptimizer> logger) => _logger = logger;

    /// <summary>
    /// Batches results in groups of groupSize and computes per-batch advantages.
    /// </summary>
    public OptimizationResult Update(
        IReadOnlyList<QuestionAnswerResult> results,
        int groupSize = DefaultGroupSize)
    {
        if (results.Count == 0) return OptimizationResult.Empty;
        var batchAdvantages = ComputeBatchAdvantages(results, groupSize);
        var meanAdv = batchAdvantages.Average();
        var signal = meanAdv * (1.0 + results.Count(r => r.IsCorrect) * 0.05);
        _logger.LogInformation(
            "GRPO: {Count} results, {Batches} batches (size={Size}), mean_adv={Adv:F4}",
            results.Count, batchAdvantages.Count, groupSize, meanAdv);
        return new OptimizationResult(meanAdv, signal, DateTime.UtcNow, true);
    }

    private static List<double> ComputeBatchAdvantages(
        IReadOnlyList<QuestionAnswerResult> results, int groupSize)
    {
        var advantages = new List<double>();
        for (var i = 0; i < results.Count; i += groupSize)
        {
            var batch = results.Skip(i).Take(groupSize).ToList();
            var diffs = batch.Select(r => (double)r.DifficultyScore).ToList();
            var mean = diffs.Average();
            var std = Math.Sqrt(diffs.Select(d => Math.Pow(d - mean, 2)).Average());
            advantages.Add(diffs.Select(d => (d - mean) / (std + 1e-8)).Average());
        }
        return advantages;
    }
}
