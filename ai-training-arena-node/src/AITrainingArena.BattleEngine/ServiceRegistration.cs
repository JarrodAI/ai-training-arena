using AITrainingArena.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AITrainingArena.BattleEngine;

/// <summary>
/// Centralizes BattleEngine layer DI registrations.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers: orchestrator, Dr. Zero engine, proposer, solver,
    /// HRPO/GRPO optimizers, IPFS search engine, and battle protocol.
    /// </summary>
    public static IServiceCollection AddBattleEngineServices(this IServiceCollection services)
    {
        services.AddScoped<IBattleOrchestrator, BattleOrchestrator>();
        services.AddScoped<IDrZeroEngine, DrZeroEngine>();
        services.AddScoped<ProposerService>();
        services.AddScoped<SolverService>();
        services.AddScoped<BattleProtocol>();
        services.AddSingleton<HRPOOptimizer>();
        services.AddSingleton<GRPOOptimizer>();
        services.AddHttpClient<IPFSSearchEngine>();
        return services;
    }
}
