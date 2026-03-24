using AITrainingArena.Blockchain;
using AITrainingArena.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AITrainingArena.Tests;

public class ProofGeneratorTests
{
    private readonly ProofGenerator _generator = new(NullLogger<ProofGenerator>.Instance);

    [Fact]
    public async Task GenerateProof_ReturnsNonEmptyBytes()
    {
        var result = TestHelpers.CreateTestBattleResult();
        var proof = await _generator.GenerateProofAsync(result);
        proof.Should().NotBeEmpty();
        proof.Length.Should().BeGreaterThan(32);
    }

    [Fact]
    public async Task VerifyProof_MatchingResult_ReturnsTrue()
    {
        var result = TestHelpers.CreateTestBattleResult();
        var proof = await _generator.GenerateProofAsync(result);
        (await _generator.VerifyProofAsync(proof, result)).Should().BeTrue();
    }

    [Fact]
    public async Task VerifyProof_WrongResult_ReturnsFalse()
    {
        var result = TestHelpers.CreateTestBattleResult();
        var proof = await _generator.GenerateProofAsync(result);
        var wrong = new BattleResult("different", 50m, 60m, 10, 7, 3m, 30m, 20m, 10m, null);
        (await _generator.VerifyProofAsync(proof, wrong)).Should().BeFalse();
    }

    [Fact]
    public async Task VerifyProof_ShortBytes_ReturnsFalse()
    {
        var result = TestHelpers.CreateTestBattleResult();
        (await _generator.VerifyProofAsync(new byte[10], result)).Should().BeFalse();
    }
}
