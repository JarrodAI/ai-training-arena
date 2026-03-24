namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Port for IPFS operations via Kubo HTTP API.
/// </summary>
public interface IIPFSClient
{
    /// <summary>Pins data to IPFS and returns the CID.</summary>
    /// <param name="data">Raw bytes to pin.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The IPFS content identifier (CID).</returns>
    Task<string> PinAsync(byte[] data, CancellationToken ct = default);

    /// <summary>Fetches data from IPFS by CID.</summary>
    /// <param name="cid">The content identifier to fetch.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<byte[]> FetchAsync(string cid, CancellationToken ct = default);
}
