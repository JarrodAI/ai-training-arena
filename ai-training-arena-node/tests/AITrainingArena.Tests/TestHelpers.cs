using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Tests;

public static class TestHelpers
{
    public static Agent CreateTestAgent(uint nftId = 1, AgentClass agentClass = AgentClass.A)
    {
        return new Agent
        {
            NftId = nftId,
            Class = agentClass,
            ModelName = "test-model",
            EloRating = EloRating.Default(),
            TotalBattles = 0,
            Wins = 0,
            IsActive = true,
            StakedAmount = 100m,
            OwnerAddress = new WalletAddress("0x0000000000000000000000000000000000000001")
        };
    }

    public static BattleResult CreateTestBattleResult()
    {
        return new BattleResult(
            Winner: "proposer",
            ProposerScore: 85.5m,
            SolverScore: 72.3m,
            TotalQuestions: 10,
            CorrectAnswers: 7,
            AvgDifficulty: 3.5m,
            ProposerReward: 50m,
            SolverReward: 30m,
            BurnAmount: 20m,
            TelemetryIpfsHash: null);
    }
}
