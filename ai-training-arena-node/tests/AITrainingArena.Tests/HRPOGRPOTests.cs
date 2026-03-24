using AITrainingArena.BattleEngine;
using AITrainingArena.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AITrainingArena.Tests;

public class HRPOGRPOTests
{
    private readonly HRPOOptimizer _hrpo = new(NullLogger<HRPOOptimizer>.Instance);
    private readonly GRPOOptimizer _grpo = new(NullLogger<GRPOOptimizer>.Instance);

    [Fact]
    public void HRPO_EmptyResults_ReturnsEmpty()
        => _hrpo.Update([]).Should().Be(OptimizationResult.Empty);

    [Fact]
    public void GRPO_EmptyResults_ReturnsEmpty()
        => _grpo.Update([]).Should().Be(OptimizationResult.Empty);

    [Fact]
    public void HRPO_WithResults_IsQueuedForFineTuning()
    {
        var results = CreateResults(6, 3);
        _hrpo.Update(results).IsQueuedForFineTuning.Should().BeTrue();
    }

    [Fact]
    public void GRPO_WithResults_IsQueuedForFineTuning()
    {
        var results = CreateResults(10, 5);
        _grpo.Update(results, groupSize: 5).IsQueuedForFineTuning.Should().BeTrue();
    }

    private static IReadOnlyList<QuestionAnswerResult> CreateResults(int total, int correct)
        => Enumerable.Range(0, total)
            .Select(i => new QuestionAnswerResult(
                $"q{i}", $"a{i}", $"a{i}", i < correct, 100L, 0.2m + i * 0.1m, 1 + (i % 3), []))
            .ToList();
}
