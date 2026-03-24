using AITrainingArena.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.Infrastructure.Ipfs;

public class IpfsClient : IIPFSClient
{
    private readonly ILogger<IpfsClient> _logger;

    public IpfsClient(ILogger<IpfsClient> logger)
    {
        _logger = logger;
    }

    public async Task<string> PinAsync(byte[] data, CancellationToken ct = default)
    {
        _logger.LogInformation("Pinning {Bytes} bytes to IPFS (placeholder)", data.Length);
        await Task.CompletedTask;
        var placeholderCid = $"Qm{Convert.ToHexString(data.AsSpan(0, Math.Min(16, data.Length))).ToLowerInvariant()}";
        _logger.LogInformation("Pinned with placeholder CID: {Cid}", placeholderCid);
        return placeholderCid;
    }

    public async Task<byte[]> FetchAsync(string cid, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching CID {Cid} from IPFS (placeholder)", cid);
        await Task.CompletedTask;
        return [];
    }
}
