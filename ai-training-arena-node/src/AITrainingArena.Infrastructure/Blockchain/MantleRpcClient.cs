using Microsoft.Extensions.Logging;
using Nethereum.Web3;

namespace AITrainingArena.Infrastructure.Blockchain;

public class MantleRpcClient
{
    private readonly Web3 _web3;
    private readonly ILogger<MantleRpcClient> _logger;
    private readonly string _ataContractAddress;

    public MantleRpcClient(string rpcUrl, string ataContractAddress, ILogger<MantleRpcClient> logger)
    {
        _web3 = new Web3(rpcUrl);
        _ataContractAddress = ataContractAddress;
        _logger = logger;
    }

    public async Task<decimal> GetBalanceAsync(string address)
    {
        _logger.LogInformation("Getting MNT balance for {Address}", address);
        var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
        return Web3.Convert.FromWei(balance.Value);
    }

    public async Task<decimal> GetATABalanceAsync(string address)
    {
        _logger.LogInformation("Getting ATA token balance for {Address} from contract {Contract}",
            address, _ataContractAddress);
        // Placeholder: real implementation requires ERC-20 balanceOf ABI call
        await Task.CompletedTask;
        return 0m;
    }

    public async Task<bool> IsConnectedAsync()
    {
        try
        {
            var blockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            _logger.LogInformation("Connected to Mantle. Current block: {Block}", blockNumber.Value);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to Mantle RPC");
            return false;
        }
    }

    public async Task<ulong> GetBlockNumberAsync()
    {
        var block = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        return (ulong)block.Value;
    }
}
