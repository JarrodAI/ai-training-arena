using AITrainingArena.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.BattleEngine;

/// <summary>
/// Generates multi-hop questions for the proposer role using a placeholder knowledge base.
/// Real AI inference will replace the hardcoded data in a future iteration.
/// </summary>
public sealed class ProposerService
{
    private readonly ILogger<ProposerService> _logger;

    private static readonly List<(string Fact, string Entity)> KnowledgeBase =
    [
        ("The capital of France is Paris.", "Paris"),
        ("Paris is known as the City of Light.", "City of Light"),
        ("The Eiffel Tower is located in Paris.", "Eiffel Tower"),
        ("The Eiffel Tower was built in 1889.", "1889"),
        ("The Seine River flows through Paris.", "Seine River"),
        ("The Louvre Museum is in Paris.", "Louvre Museum"),
        ("The Mona Lisa is displayed in the Louvre.", "Mona Lisa"),
        ("Leonardo da Vinci painted the Mona Lisa.", "Leonardo da Vinci"),
        ("Leonardo da Vinci was born in Vinci, Italy.", "Vinci, Italy"),
        ("Italy is a country in Southern Europe.", "Southern Europe"),
        ("The Mediterranean Sea borders Southern Europe.", "Mediterranean Sea"),
        ("Bitcoin was created by Satoshi Nakamoto.", "Satoshi Nakamoto"),
        ("Ethereum was proposed by Vitalik Buterin in 2013.", "Vitalik Buterin"),
        ("Smart contracts run on the Ethereum Virtual Machine.", "Ethereum Virtual Machine"),
        ("Solidity is the primary language for Ethereum smart contracts.", "Solidity"),
    ];

    public ProposerService(ILogger<ProposerService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates a multi-hop question by chaining facts from the placeholder knowledge base.
    /// </summary>
    /// <param name="hopCount">Number of reasoning hops (1-5). Clamped to available facts.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="Question"/> with hop chain and expected answer.</returns>
    public Task<Question> GenerateMultiHopQuestionAsync(int hopCount, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var clampedHops = Math.Clamp(hopCount, 1, Math.Min(5, KnowledgeBase.Count));
        _logger.LogDebug("Generating {HopCount}-hop question from placeholder knowledge base", clampedHops);

        var random = Random.Shared;
        var startIndex = random.Next(0, KnowledgeBase.Count - clampedHops);
        var hopChain = new List<string>();
        var questionParts = new List<string>();

        for (var i = 0; i < clampedHops; i++)
        {
            var (fact, _) = KnowledgeBase[startIndex + i];
            hopChain.Add(fact);
            questionParts.Add(fact);
        }

        var (_, expectedAnswer) = KnowledgeBase[startIndex + clampedHops - 1];
        var difficultyScore = CalculateBaseDifficulty(clampedHops);

        var questionText = $"Based on the following chain of facts, what is the key entity? " +
                           string.Join(" -> ", questionParts);

        var question = new Question
        {
            Text = questionText,
            HopCount = clampedHops,
            ExpectedAnswer = expectedAnswer,
            DifficultyScore = difficultyScore,
            HopChain = hopChain,
            GeneratedAt = DateTime.UtcNow,
        };

        _logger.LogDebug("Generated question with difficulty {Difficulty}", difficultyScore);
        return Task.FromResult(question);
    }

    private static decimal CalculateBaseDifficulty(int hopCount)
    {
        return hopCount switch
        {
            1 => 0.2m,
            2 => 0.4m,
            3 => 0.6m,
            4 => 0.8m,
            _ => 1.0m,
        };
    }
}
