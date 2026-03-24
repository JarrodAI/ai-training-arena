using AITrainingArena.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;

namespace AITrainingArena.Blockchain;

/// <summary>
/// Centralizes Blockchain layer DI registrations.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers blockchain services: ProofGenerator, ResultSubmitter, Nethereum Web3.
    /// </summary>
    public static IServiceCollection AddBlockchainServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSingleton(sp =>
        {
            var rpcUrl = config["NodeConfiguration:MantleRpcUrl"] ?? "https://rpc.mantle.xyz";
            return new Web3(rpcUrl);
        });

        services.AddSingleton<IProofGenerator, ProofGenerator>();

        services.AddSingleton<IResultSubmitter>(sp =>
        {
            var web3 = sp.GetRequiredService<Web3>();
            var logger = sp.GetRequiredService<ILogger<ResultSubmitter>>();
            var verifierAddr = config["Blockchain:BattleVerifierAddress"] ?? string.Empty;
            var walletAddr = config["NodeConfiguration:WalletAddress"] ?? string.Empty;
            var privateKey = config["NodeConfiguration:WalletPrivateKey"] ?? string.Empty;
            return new ResultSubmitter(web3, verifierAddr, walletAddr, privateKey, logger);
        });

        return services;
    }
}
