namespace AITrainingArena.Domain.ValueObjects;

/// <summary>
/// Result of a single question-answer exchange within a battle round.
/// </summary>
/// <param name="QuestionText">The multi-hop question text.</param>
/// <param name="ExpectedAnswer">The correct answer to the question.</param>
/// <param name="SolverAnswer">The solver's submitted answer.</param>
/// <param name="IsCorrect">Whether the solver's answer matched the expected answer.</param>
/// <param name="ResponseTimeMs">Time taken by the solver to respond in milliseconds.</param>
/// <param name="DifficultyScore">Computed difficulty of the question.</param>
/// <param name="HopCount">Number of reasoning hops in the question.</param>
/// <param name="SearchQueries">Search queries used by the solver, if any.</param>
public record QuestionAnswerResult(
    string QuestionText,
    string ExpectedAnswer,
    string SolverAnswer,
    bool IsCorrect,
    long ResponseTimeMs,
    decimal DifficultyScore,
    int HopCount,
    IReadOnlyList<string> SearchQueries);
