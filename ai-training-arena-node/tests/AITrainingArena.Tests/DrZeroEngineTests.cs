using AITrainingArena.BattleEngine;
using AITrainingArena.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AITrainingArena.Tests;

public class DrZeroEngineTests
{
    [Fact]
    public void VerifyAnswer_ExactMatch_ReturnsTrue()
    {
        var q = new Question { ExpectedAnswer = "Paris", Text = "city?", HopCount = 1 };
        DrZeroEngine.VerifyAnswer(q, "Paris").Should().BeTrue();
    }

    [Fact]
    public void VerifyAnswer_CaseInsensitive_ReturnsTrue()
    {
        var q = new Question { ExpectedAnswer = "paris", Text = "city?", HopCount = 1 };
        DrZeroEngine.VerifyAnswer(q, "PARIS").Should().BeTrue();
    }

    [Fact]
    public void VerifyAnswer_ContainsExpected_ReturnsTrue()
    {
        var q = new Question { ExpectedAnswer = "Paris", Text = "city?", HopCount = 1 };
        DrZeroEngine.VerifyAnswer(q, "The answer is Paris.").Should().BeTrue();
    }

    [Fact]
    public void VerifyAnswer_WrongAnswer_ReturnsFalse()
    {
        var q = new Question { ExpectedAnswer = "Paris", Text = "city?", HopCount = 1 };
        DrZeroEngine.VerifyAnswer(q, "Berlin").Should().BeFalse();
    }

    [Fact]
    public void VerifyAnswer_EmptyAnswer_ReturnsFalse()
    {
        var q = new Question { ExpectedAnswer = "Paris", Text = "city?", HopCount = 1 };
        DrZeroEngine.VerifyAnswer(q, string.Empty).Should().BeFalse();
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    public void CalculateDifficulty_BetweenZeroAndOne(int hops, bool correct)
    {
        var q = new Question { HopCount = hops, Text = "test", ExpectedAnswer = "x" };
        var d = DrZeroEngine.CalculateDifficulty(q, "answer", correct);
        d.Should().BeInRange(0.0m, 1.0m);
    }
}
