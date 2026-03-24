using AITrainingArena.Domain.Interfaces;
using AITrainingArena.Infrastructure.Blockchain;
using AITrainingArena.Infrastructure.Data;
using AITrainingArena.Infrastructure.Ipfs;
using AITrainingArena.Infrastructure.Messaging;
using AITrainingArena.Infrastructure.Models;
using AITrainingArena.Infrastructure.P2P;
using AITrainingArena.Infrastructure.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.Infrastructure;

/// <summary>
/// Centralizes Infrastructure layer DI registrations.
/// All domain interface ports are wired to their adapter implementations here.
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration config)
    {
        RegisterDatabase(services, config);
        RegisterBlockchain(services, config);
        RegisterIpfs(services);
        RegisterTelemetry(services);
        RegisterModels(services);
        RegisterNetworking(services, config);
        RegisterMessaging(services, config);
        return services;
    }

    private static void RegisterDatabase(IServiceCollection services, IConfiguration config)
    {
        var sqlitePath = config["Database:Path"] ?? "arena.db";
        services.AddDbContext<ArenaDbContext>(opts =>
            opts.UseSqlite($"Data Source={sqlitePath}"));
        services.AddScoped<AgentRepository>();
        services.AddScoped<BattleRepository>();
        services.AddScoped<IAgentRepository>(sp => sp.GetRequiredService<AgentRepository>());
        services.AddScoped<IBattleRepository>(sp => sp.GetRequiredService<BattleRepository>());
    }

    private static void RegisterBlockchain(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<MantleRpcClient>(sp =>
        {
            var rpcUrl = config["NodeConfiguration:MantleRpcUrl"]
                ?? config["Blockchain:RpcUrl"]
                ?? "https://rpc.mantle.xyz";
            var ataContract = config["Blockchain:ATAContractAddress"] ?? string.Empty;
            var logger = sp.GetRequiredService<ILogger<MantleRpcClient>>();
            return new MantleRpcClient(rpcUrl, ataContract, logger);
        });
        services.AddSingleton<ContractInteraction>();
    }

    private static void RegisterIpfs(IServiceCollection services)
        => services.AddSingleton<IIPFSClient, IpfsClient>();

    private static void RegisterTelemetry(IServiceCollection services)
    {
        services.AddSingleton<ITelemetryService, TelemetryService>();
        services.AddSingleton<TelemetryEncryption>();
    }

    private static void RegisterModels(IServiceCollection services)
    {
        services.AddScoped<ModelRegistry>();
        services.AddScoped<IModelRegistry>(sp => sp.GetRequiredService<ModelRegistry>());
    }

    private static void RegisterNetworking(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<INetworkManager>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<NetworkManager>>();
            var peerId = config["NodeConfiguration:WalletAddress"]
                         ?? Guid.NewGuid().ToString("N");
            var port = int.TryParse(config["NodeConfiguration:P2PPort"], out var p) ? p : 4001;
            var bootstrapPeers = config
                .GetSection("NodeConfiguration:BootstrapPeers")
                .GetChildren()
                .Select(c => c.Value ?? string.Empty)
                .Where(v => !string.IsNullOrEmpty(v))
                .ToArray();
            return new NetworkManager(logger, peerId, port, bootstrapPeers);
        });
    }

    private static void RegisterMessaging(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<NatsEventPublisher>(sp =>
        {
            var natsUrl = config["Messaging:NatsUrl"] ?? "nats://localhost:4222";
            var logger = sp.GetRequiredService<ILogger<NatsEventPublisher>>();
            return new NatsEventPublisher(natsUrl, logger);
        });
    }
}
