using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.BattleEngine;

/// <summary>
/// Dr. Zero engine implementation: generates multi-hop questions and verifies answers.
/// Delegates question generation to <see cref="ProposerService"/> and answer solving to <see cref="SolverService"/>.
/// </summary>
public sealed class DrZeroEngine : IDrZeroEngine
{
    private readonly ProposerService _proposerService;
    private readonly SolverService _solverService;
    private readonly ILogger<DrZeroEngine> _logger;

    public DrZeroEngine(
        ProposerService proposerService,
        SolverService solverService,
        ILogger<DrZeroEngine> logger)
    {
        _proposerService = proposerService;
        _solverService = solverService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Question> GenerateQuestionAsync(int hopCount, CancellationToken ct = default)
    {
        _logger.LogInformation("Dr. Zero generating {HopCount}-hop question", hopCount);
        var question = await _proposerService.GenerateMultiHopQuestionAsync(hopCount, ct);
        _logger.LogInformation("Generated question with difficulty {Difficulty}", question.DifficultyScore);
        return question;
    }

    /// <inheritdoc />
    public Task<string> EvaluateAnswerAsync(Question question, string answer, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _logger.LogInformation("Dr. Zero evaluating answer for question (hop={HopCount})", question.HopCount);

        var isCorrect = VerifyAnswer(question, answer);
        var difficulty = CalculateDifficulty(question, answer, isCorrect);

        var evaluation = isCorrect
            ? $"CORRECT|difficulty={difficulty:F2}"
            : $"INCORRECT|difficulty={difficulty:F2}|expected={question.ExpectedAnswer}";

        _logger.LogInformation("Evaluation result: {Result}", evaluation);
        return Task.FromResult(evaluation);
    }

    /// <summary>
    /// Verifies an answer against the expected answer using fuzzy matching.
    /// Performs case-insensitive comparison and checks for substring containment.
    /// </summary>
    internal static bool VerifyAnswer(Question question, string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
            return false;

        var expected = question.ExpectedAnswer.Trim();
        var actual = answer.Trim();

        if (string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
            return true;

        if (actual.Contains(expected, StringComparison.OrdinalIgnoreCase))
            return true;

        if (expected.Contains(actual, StringComparison.OrdinalIgnoreCase) && actual.Length >= 3)
            return true;

        return false;
    }

    /// <summary>
    /// Calculates difficulty based on hop count and whether the answer was correct.
    /// Higher hop counts and incorrect answers indicate harder questions.
    /// </summary>
    internal static decimal CalculateDifficulty(Question question, string answer, bool correct)
    {
        var baseDifficulty = question.HopCount * 0.2m;
        var correctnessModifier = correct ? -0.1m : 0.1m;
        var lengthModifier = string.IsNullOrEmpty(answer) ? 0.05m : 0m;

        return Math.Clamp(baseDifficulty + correctnessModifier + lengthModifier, 0.1m, 1.0m);
    }
}
