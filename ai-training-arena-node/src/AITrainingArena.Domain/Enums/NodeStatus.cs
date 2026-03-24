namespace AITrainingArena.Domain.Enums;

/// <summary>
/// P2P node availability status for matchmaking registry.
/// </summary>
public enum NodeStatus
{
    /// <summary>Node is not connected to the network.</summary>
    Offline,
    /// <summary>Node is online and ready for matchmaking.</summary>
    Available,
    /// <summary>Node is currently engaged in a battle.</summary>
    InBattle
}
