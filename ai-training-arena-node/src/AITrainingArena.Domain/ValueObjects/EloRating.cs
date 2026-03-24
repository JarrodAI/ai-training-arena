namespace AITrainingArena.Domain.ValueObjects;

/// <summary>
/// Immutable Elo rating value object with K-factor calculation.
/// Starting Elo: 1500. K=40 for agents with fewer than 30 battles, K=20 otherwise.
/// </summary>
public readonly record struct EloRating
{
    /// <summary>Default starting Elo rating for all new agents.</summary>
    public const int DefaultRating = 1500;

    /// <summary>K-factor for agents with fewer than 30 battles.</summary>
    public const int HighKFactor = 40;

    /// <summary>K-factor for agents with 30 or more battles.</summary>
    public const int LowKFactor = 20;

    /// <summary>Battle count threshold for K-factor reduction.</summary>
    public const int KFactorThreshold = 30;

    /// <summary>The current Elo rating value.</summary>
    public int Value { get; }

    /// <summary>
    /// Creates a new <see cref="EloRating"/> with the specified value.
    /// </summary>
    /// <param name="value">The Elo rating. Must be non-negative.</param>
    public EloRating(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Elo rating cannot be negative.");
        Value = value;
    }

    /// <summary>
    /// Returns the K-factor based on the agent's total battle count.
    /// </summary>
    /// <param name="totalBattles">Number of battles the agent has completed.</param>
    public static int GetKFactor(int totalBattles) =>
        totalBattles < KFactorThreshold ? HighKFactor : LowKFactor;

    /// <summary>
    /// Calculates the expected score against an opponent.
    /// </summary>
    /// <param name="opponentRating">The opponent's Elo rating.</param>
    public double ExpectedScore(EloRating opponentRating) =>
        1.0 / (1.0 + Math.Pow(10, (opponentRating.Value - Value) / 400.0));

    /// <summary>
    /// Computes a new Elo rating after a match result.
    /// </summary>
    /// <param name="opponentRating">The opponent's Elo rating.</param>
    /// <param name="actualScore">1.0 for win, 0.5 for draw, 0.0 for loss.</param>
    /// <param name="totalBattles">Agent's total battle count (determines K-factor).</param>
    public EloRating CalculateNew(EloRating opponentRating, double actualScore, int totalBattles)
    {
        var k = GetKFactor(totalBattles);
        var expected = ExpectedScore(opponentRating);
        var newRating = (int)Math.Round(Value + k * (actualScore - expected));
        return new EloRating(Math.Max(0, newRating));
    }

    /// <summary>Creates a default starting Elo rating of 1500.</summary>
    public static EloRating Default() => new(DefaultRating);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <summary>Implicit conversion to int.</summary>
    public static implicit operator int(EloRating rating) => rating.Value;
}
