namespace AITrainingArena.Domain.ValueObjects;

/// <summary>
/// Represents an active P2P connection to a remote peer.
/// </summary>
/// <param name="PeerId">LibP2P peer identifier of the connected node.</param>
/// <param name="RemoteAddress">Multiaddress of the remote peer.</param>
/// <param name="ConnectedAt">When the connection was established.</param>
/// <param name="IsActive">Whether the connection is currently alive.</param>
public record PeerConnection(
    string PeerId,
    string RemoteAddress,
    DateTime ConnectedAt,
    bool IsActive);
