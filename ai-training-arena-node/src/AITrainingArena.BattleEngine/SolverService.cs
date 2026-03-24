using AITrainingArena.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.BattleEngine;

/// <summary>
/// Placeholder solver that attempts to answer multi-hop questions.
/// Real AI inference will replace this in a future iteration.
/// Supports up to 5 search turns (placeholder concept).
/// </summary>
public sealed class SolverService
{
    private const int MaxSearchTurns = 5;

    private readonly ILogger<SolverService> _logger;

    private static readonly Dictionary<string, string> KnownAnswers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Paris"] = "Paris",
        ["City of Light"] = "City of Light",
        ["Eiffel Tower"] = "Eiffel Tower",
        ["1889"] = "1889",
        ["Seine River"] = "Seine River",
        ["Louvre Museum"] = "Louvre Museum",
        ["Mona Lisa"] = "Mona Lisa",
        ["Leonardo da Vinci"] = "Leonardo da Vinci",
        ["Vinci, Italy"] = "Vinci, Italy",
        ["Southern Europe"] = "Southern Europe",
        ["Mediterranean Sea"] = "Mediterranean Sea",
        ["Satoshi Nakamoto"] = "Satoshi Nakamoto",
        ["Vitalik Buterin"] = "Vitalik Buterin",
        ["Ethereum Virtual Machine"] = "Ethereum Virtual Machine",
        ["Solidity"] = "Solidity",
    };

    public SolverService(ILogger<SolverService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Attempts to answer a question using placeholder keyword matching and search turns.
    /// </summary>
    /// <param name="question">The question to solve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The best-effort answer string.</returns>
    public Task<string> SolveQuestionAsync(Question question, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _logger.LogDebug("Attempting to solve {HopCount}-hop question", question.HopCount);

        var searchQueries = new List<string>();
        string? bestAnswer = null;

        for (var turn = 0; turn < MaxSearchTurns && bestAnswer is null; turn++)
        {
            var query = ExtractSearchQuery(question.Text, turn);
            searchQueries.Add(query);

            foreach (var (keyword, answer) in KnownAnswers)
            {
                if (question.Text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    bestAnswer = answer;
                }
            }
        }

        var result = bestAnswer ?? "Unknown";
        _logger.LogDebug(
            "Solver produced answer '{Answer}' after {Turns} search turns",
            result,
            searchQueries.Count);

        return Task.FromResult(result);
    }

    private static string ExtractSearchQuery(string questionText, int turn)
    {
        var words = questionText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var startIdx = Math.Min(turn * 5, Math.Max(0, words.Length - 5));
        var count = Math.Min(5, words.Length - startIdx);
        return string.Join(' ', words.AsSpan(startIdx, count).ToArray());
    }
}
