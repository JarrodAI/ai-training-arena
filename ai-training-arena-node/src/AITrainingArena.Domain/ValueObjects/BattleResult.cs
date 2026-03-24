namespace AITrainingArena.Domain.ValueObjects;

/// <summary>
/// Immutable result of a completed battle including scores, rewards, and telemetry hash.
/// </summary>
/// <param name="Winner">Peer ID of the winning agent.</param>
/// <param name="ProposerScore">Proposer's final score (difficulty*0.6 + count*0.4).</param>
/// <param name="SolverScore">Solver's final score (correct*1.5).</param>
/// <param name="TotalQuestions">Total number of questions asked in the battle.</param>
/// <param name="CorrectAnswers">Number of questions the solver answered correctly.</param>
/// <param name="AvgDifficulty">Average difficulty across all questions.</param>
/// <param name="ProposerReward">ATA reward earned by the proposer.</param>
/// <param name="SolverReward">ATA reward earned by the solver.</param>
/// <param name="BurnAmount">ATA burned (2% of total reward).</param>
/// <param name="TelemetryIpfsHash">IPFS CID of the encrypted telemetry data.</param>
public record BattleResult(
    string Winner,
    decimal ProposerScore,
    decimal SolverScore,
    int TotalQuestions,
    int CorrectAnswers,
    decimal AvgDifficulty,
    decimal ProposerReward,
    decimal SolverReward,
    decimal BurnAmount,
    string? TelemetryIpfsHash);
