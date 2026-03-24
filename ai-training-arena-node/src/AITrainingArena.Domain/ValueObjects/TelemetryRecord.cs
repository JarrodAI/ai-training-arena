using AITrainingArena.Domain.Enums;

namespace AITrainingArena.Domain.ValueObjects;

/// <summary>
/// Full telemetry record for a completed battle, matching the canonical JSON schema.
/// Stored encrypted on IPFS at ipfs://QmUser{nftId}/{battleId}/.
/// </summary>
/// <param name="BattleId">Unique battle identifier.</param>
/// <param name="Timestamp">When the battle occurred.</param>
/// <param name="Class">Agent class tier of the battle.</param>
/// <param name="Proposer">NFT ID of the proposer agent.</param>
/// <param name="Solver">NFT ID of the solver agent.</param>
/// <param name="Rounds">Ordered list of question-answer rounds.</param>
/// <param name="Result">Final battle result with scores and rewards.</param>
/// <param name="TelemetryIpfsHash">IPFS CID of this telemetry record.</param>
/// <param name="AggregateIncluded">Whether this record was included in the DAO aggregate.</param>
public record TelemetryRecord(
    string BattleId,
    DateTime Timestamp,
    AgentClass Class,
    string Proposer,
    string Solver,
    IReadOnlyList<TelemetryRound> Rounds,
    BattleResult Result,
    string? TelemetryIpfsHash,
    bool AggregateIncluded);

/// <summary>
/// A single round within a telemetry record.
/// </summary>
/// <param name="Question">The question details for this round.</param>
/// <param name="SolverResponse">The solver's response details.</param>
public record TelemetryRound(
    TelemetryQuestion Question,
    TelemetrySolverResponse SolverResponse);

/// <summary>
/// Question details within a telemetry round.
/// </summary>
/// <param name="HopCount">Number of reasoning hops.</param>
/// <param name="Difficulty">Difficulty score of the question.</param>
/// <param name="Text">The question text.</param>
/// <param name="Answer">The expected correct answer.</param>
public record TelemetryQuestion(
    int HopCount,
    decimal Difficulty,
    string Text,
    string Answer);

/// <summary>
/// Solver response details within a telemetry round.
/// </summary>
/// <param name="Answer">The solver's submitted answer.</param>
/// <param name="Correct">Whether the answer was correct.</param>
/// <param name="ResponseTimeMs">Response time in milliseconds.</param>
/// <param name="SearchQueries">Search queries used by the solver.</param>
public record TelemetrySolverResponse(
    string Answer,
    bool Correct,
    long ResponseTimeMs,
    IReadOnlyList<string> SearchQueries);
