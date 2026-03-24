using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Events;
using AITrainingArena.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AITrainingArena.Tests;

public class AgentEntityTests
{
    private static readonly WalletAddress TestOwner =
        new WalletAddress("0x1111111111111111111111111111111111111111");

    [Fact]
    public void Agent_Register_RaisesAgentRegisteredEvent()
    {
        var agent = Agent.Register(1, AgentClass.A, "qwen-7b", TestOwner);
        agent.DomainEvents.Should().ContainSingle(e => e is AgentRegistered);
        agent.EloRating.Value.Should().Be(1500);
        agent.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Agent_RecordBattleResult_Win_UpdatesElo()
    {
        var agent = Agent.Register(1, AgentClass.A, "model", TestOwner);
        var initialElo = agent.EloRating;
        agent.RecordBattleResult(EloRating.Default(), won: true);
        agent.EloRating.Value.Should().BeGreaterThan(initialElo.Value);
        agent.Wins.Should().Be(1);
        agent.TotalBattles.Should().Be(1);
    }

    [Fact]
    public void Agent_RecordBattleResult_Loss_DecreasesElo()
    {
        var agent = Agent.Register(1, AgentClass.A, "model", TestOwner);
        var initialElo = agent.EloRating;
        agent.RecordBattleResult(EloRating.Default(), won: false);
        agent.EloRating.Value.Should().BeLessThan(initialElo.Value);
        agent.Wins.Should().Be(0);
        agent.TotalBattles.Should().Be(1);
    }

    [Fact]
    public void Stake_GetMinimumStake_ReturnsCorrectValues()
    {
        Stake.GetMinimumStake(AgentClass.A).Should().Be(100m);
        Stake.GetMinimumStake(AgentClass.B).Should().Be(500m);
        Stake.GetMinimumStake(AgentClass.C).Should().Be(2000m);
        Stake.GetMinimumStake(AgentClass.D).Should().Be(8000m);
        Stake.GetMinimumStake(AgentClass.E).Should().Be(30000m);
    }
}
