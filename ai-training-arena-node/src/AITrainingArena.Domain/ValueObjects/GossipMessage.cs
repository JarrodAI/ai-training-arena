namespace AITrainingArena.Domain.ValueObjects;

/// <summary>
/// Message exchanged between peers via gossip protocol for leaderboard synchronization.
/// </summary>
/// <param name="MessageType">Type of gossip message.</param>
/// <param name="Payload">Serialized message payload.</param>
/// <param name="OriginPeer">Peer ID of the message originator.</param>
/// <param name="Timestamp">When the message was created.</param>
public record GossipMessage(
    GossipMessageType MessageType,
    byte[] Payload,
    string OriginPeer,
    DateTime Timestamp);

/// <summary>
/// Types of gossip messages exchanged between peers.
/// </summary>
public enum GossipMessageType
{
    /// <summary>Notification of a newly completed battle result.</summary>
    NewBattleResult,
    /// <summary>Full leaderboard synchronization request/response.</summary>
    LeaderboardSync
}
