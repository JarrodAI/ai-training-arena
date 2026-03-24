using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.BattleEngine;

/// <summary>
/// Static utility for computing Elo rating changes after battles.
/// K-factor: 40 if totalBattles less than 30, else 20.
/// Expected score: E = 1/(1+10^((Rb-Ra)/400)).
/// Minimum Elo clamped to 100.
/// </summary>
public static class EloCalculator
{
    private const int MinimumElo = 100;

    /// <summary>
    /// Calculates the new Elo rating for a player after a battle.
    /// </summary>
    /// <param name="playerElo">Current Elo rating of the player.</param>
    /// <param name="opponentElo">Current Elo rating of the opponent.</param>
    /// <param name="won">Whether the player won the battle.</param>
    /// <param name="totalBattles">Total battles the player has completed (determines K-factor).</param>
    /// <returns>The new Elo rating, clamped to a minimum of 100.</returns>
    public static int CalculateNewElo(int playerElo, int opponentElo, bool won, int totalBattles)
    {
        var kFactor = EloRating.GetKFactor(totalBattles);
        var expectedScore = 1.0 / (1.0 + Math.Pow(10, (opponentElo - playerElo) / 400.0));
        var actualScore = won ? 1.0 : 0.0;
        var delta = kFactor * (actualScore - expectedScore);
        var newElo = (int)Math.Round(playerElo + delta);
        return Math.Max(MinimumElo, newElo);
    }
}
