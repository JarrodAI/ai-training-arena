using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Events;
using AITrainingArena.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AITrainingArena.Tests;

public class BattleEntityTests
{
    [Fact]
    public void Battle_Create_InitializesPending()
    {
        var battle = Battle.Create("proposer-1", "solver-1", BattleRole.Proposer);
        battle.Status.Should().Be(BattleStatus.Pending);
        battle.BattleId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Battle_Start_TransitionsToInProgress()
    {
        var battle = Battle.Create("p", "s", BattleRole.Proposer);
        battle.Start();
        battle.Status.Should().Be(BattleStatus.InProgress);
    }

    [Fact]
    public void Battle_StartTwice_Throws()
    {
        var battle = Battle.Create("p", "s", BattleRole.Proposer);
        battle.Start();
        battle.Invoking(b => b.Start()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Battle_Complete_RaisesBattleCompletedEvent()
    {
        var battle = Battle.Create("p", "s", BattleRole.Proposer);
        battle.Start();
        battle.BeginScoring();
        battle.Complete(TestHelpers.CreateTestBattleResult());
        battle.DomainEvents.Should().ContainSingle(e => e is BattleCompleted);
    }

    [Fact]
    public void Battle_Cancel_SetsCancelled()
    {
        var battle = Battle.Create("p", "s", BattleRole.Proposer);
        battle.Cancel();
        battle.Status.Should().Be(BattleStatus.Cancelled);
    }

    [Fact]
    public void Battle_AddRound_IncrementsCount()
    {
        var battle = Battle.Create("p", "s", BattleRole.Proposer);
        battle.Start();
        battle.AddRound(new QuestionAnswerResult("q?", "a", "a", true, 100, 0.5m, 2, []));
        battle.Rounds.Should().HaveCount(1);
    }
}
