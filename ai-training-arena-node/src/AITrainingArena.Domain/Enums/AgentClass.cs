namespace AITrainingArena.Domain.Enums;

/// <summary>
/// Agent class tiers based on model parameter count.
/// A=3B-7B, B=7B-32B, C=32B-70B, D=70B-405B, E=405B+.
/// </summary>
public enum AgentClass
{
    /// <summary>3B-7B parameters. Supply: 15,000. Multiplier: 1.0x.</summary>
    A,
    /// <summary>7B-32B parameters. Supply: 6,000. Multiplier: 1.2x.</summary>
    B,
    /// <summary>32B-70B parameters. Supply: 2,500. Multiplier: 1.5x.</summary>
    C,
    /// <summary>70B-405B parameters. Supply: 1,200. Multiplier: 2.0x.</summary>
    D,
    /// <summary>405B+ parameters. Supply: 300. Multiplier: 3.0x.</summary>
    E
}
