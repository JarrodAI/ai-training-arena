using AITrainingArena.Domain.Enums;

namespace AITrainingArena.API;

/// <summary>
/// Strongly-typed configuration for the AI Training Arena P2P node.
/// Bound from appsettings.json via IOptions pattern.
/// </summary>
public class NodeConfiguration
{
    /// <summary>Configuration section name in appsettings.json.</summary>
    public const string SectionName = "Node";

    /// <summary>Ethereum wallet address for this node.</summary>
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>Encrypted wallet private key (encrypted at rest).</summary>
    public string WalletPrivateKey { get; set; } = string.Empty;

    /// <summary>Path to the ONNX model file.</summary>
    public string ModelPath { get; set; } = string.Empty;

    /// <summary>Agent class (A through E).</summary>
    public AgentClass AgentClass { get; set; } = AgentClass.A;

    /// <summary>NFT token ID for this agent.</summary>
    public uint NftId { get; set; }

    /// <summary>Whether to automatically enter battles.</summary>
    public bool AutoBattle { get; set; } = true;

    /// <summary>P2P network port.</summary>
    public int P2PPort { get; set; } = 4001;

    /// <summary>WebSocket port for frontend connection.</summary>
    public int WebSocketPort { get; set; } = 8080;

    /// <summary>HTTP API port for frontend REST queries.</summary>
    public int HttpApiPort { get; set; } = 8081;

    /// <summary>Mantle L2 RPC endpoint URL.</summary>
    public string MantleRpcUrl { get; set; } = "https://rpc.mantle.xyz";

    /// <summary>IPFS Kubo API multiaddr.</summary>
    public string IpfsApiUrl { get; set; } = "/ip4/127.0.0.1/tcp/5001";

    /// <summary>DHT bootstrap peer addresses.</summary>
    public string[] BootstrapPeers { get; set; } = [];

    /// <summary>Local data storage directory.</summary>
    public string DataDirectory { get; set; } = "./data";

    /// <summary>Fall back to cloud GPU when no local GPU available.</summary>
    public bool CloudFallback { get; set; }

    /// <summary>Cloud API key (AWS Bedrock or Azure OpenAI).</summary>
    public string CloudApiKey { get; set; } = string.Empty;
}
