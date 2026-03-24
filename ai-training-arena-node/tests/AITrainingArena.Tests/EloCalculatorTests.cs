using AITrainingArena.BattleEngine;
using AITrainingArena.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AITrainingArena.Tests;

public class EloCalculatorTests
{
    [Fact]
    public void CalculateNewElo_WinnerGainsRating()
    {
        var newElo = EloCalculator.CalculateNewElo(1500, 1500, won: true, totalBattles: 5);
        newElo.Should().BeGreaterThan(1500);
    }

    [Fact]
    public void CalculateNewElo_LoserLosesRating()
    {
        var newElo = EloCalculator.CalculateNewElo(1500, 1500, won: false, totalBattles: 5);
        newElo.Should().BeLessThan(1500);
    }

    [Fact]
    public void CalculateNewElo_KFactor40_FewBattles_Delta20()
    {
        var winner = EloCalculator.CalculateNewElo(1500, 1500, true, 10);
        (winner - 1500).Should().Be(20);
    }

    [Fact]
    public void CalculateNewElo_KFactor20_ManyBattles_Delta10()
    {
        var winner = EloCalculator.CalculateNewElo(1500, 1500, true, 30);
        (winner - 1500).Should().Be(10);
    }

    [Fact]
    public void EloRating_DefaultIs1500()
        => EloRating.Default().Value.Should().Be(1500);
}
