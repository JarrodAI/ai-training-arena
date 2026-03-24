using AITrainingArena.Domain.Entities;

namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Port for the Dr. Zero AI engine that generates questions and evaluates answers.
/// </summary>
public interface IDrZeroEngine
{
    /// <summary>Generates a multi-hop question with the specified hop count.</summary>
    /// <param name="hopCount">Number of reasoning hops required.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Question> GenerateQuestionAsync(int hopCount, CancellationToken ct = default);

    /// <summary>Evaluates a solver's answer against the expected answer.</summary>
    /// <param name="question">The question that was asked.</param>
    /// <param name="answer">The solver's submitted answer.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<string> EvaluateAnswerAsync(Question question, string answer, CancellationToken ct = default);
}
