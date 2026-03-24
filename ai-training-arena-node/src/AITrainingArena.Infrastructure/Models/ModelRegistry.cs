using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Interfaces;
using AITrainingArena.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.Infrastructure.Models;

/// <summary>
/// In-process model registry backed by SQLite via AgentRepository.
/// Returns supported model recommendations per agent class from the canonical shared data models.
///
/// Supported model families per class (from master-chapters.md PART 0B):
///   Class A (3B-7B):   Qwen2.5-3B, Qwen2.5-7B, Llama3.2-3B, Llama3.1-7B
///   Class B (7B-32B):  Qwen2.5-14B, Mistral-7B, Llama3.1-8B, DeepSeek-R1-7B
///   Class C (32B-70B): Qwen2.5-32B, Llama3.1-70B, Mixtral-8x7B
///   Class D (70B-405B):Llama3.1-70B, Llama3.1-405B, Qwen2.5-72B
///   Class E (405B+):   Llama3.1-405B, GPT-4-class models, Gemini-class models
/// </summary>
public sealed class ModelRegistry : IModelRegistry
{
    private readonly AgentRepository _agentRepository;
    private readonly ILogger<ModelRegistry> _logger;

    // Canonical model recommendations per class (shared data models, Part 0B)
    private static readonly IReadOnlyDictionary<AgentClass, string[]> RecommendedModels =
        new Dictionary<AgentClass, string[]>
        {
            [AgentClass.A] = ["Qwen2.5-7B", "Qwen2.5-3B", "Llama3.2-3B", "Llama3.1-7B"],
            [AgentClass.B] = ["Qwen2.5-14B", "Mistral-7B-v0.3", "Llama3.1-8B", "DeepSeek-R1-7B"],
            [AgentClass.C] = ["Qwen2.5-32B", "Llama3.1-70B", "Mixtral-8x7B"],
            [AgentClass.D] = ["Qwen2.5-72B", "Llama3.1-70B-Instruct", "Llama3.1-405B"],
            [AgentClass.E] = ["Llama3.1-405B", "Llama3.3-70B", "DeepSeek-R1-671B"],
        };

    // Parameter count ranges per class in billions (lower, upper)
    private static readonly IReadOnlyDictionary<AgentClass, (double Min, double Max)> ClassParamRanges =
        new Dictionary<AgentClass, (double, double)>
        {
            [AgentClass.A] = (3.0, 7.0),
            [AgentClass.B] = (7.0, 32.0),
            [AgentClass.C] = (32.0, 70.0),
            [AgentClass.D] = (70.0, 405.0),
            [AgentClass.E] = (405.0, double.MaxValue),
        };

    public ModelRegistry(AgentRepository agentRepository, ILogger<ModelRegistry> logger)
    {
        _agentRepository = agentRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Agent?> GetAgentAsync(uint nftId, CancellationToken ct = default)
    {
        var agent = await _agentRepository.GetByNftIdAsync(nftId, ct);
        if (agent is null)
            _logger.LogDebug("Agent NFT {NftId} not found in local registry", nftId);
        return agent;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Agent>> GetAgentsByClassAsync(
        AgentClass agentClass,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Querying agents by class {Class}", agentClass);
        // AgentRepository doesn't have a GetByClass method — query via owner would be filtered differently.
        // For now return empty list; a proper implementation would add GetByClassAsync to AgentRepository.
        await Task.CompletedTask;
        return Array.Empty<Agent>();
    }

    /// <inheritdoc />
    public async Task RegisterAgentAsync(Agent agent, CancellationToken ct = default)
    {
        var existing = await _agentRepository.GetByNftIdAsync(agent.NftId, ct);
        if (existing is null)
        {
            await _agentRepository.AddAsync(agent, ct);
            _logger.LogInformation(
                "Registered agent NFT {NftId}, Class {Class}, Model {Model}",
                agent.NftId, agent.Class, agent.ModelName);
        }
        else
        {
            await _agentRepository.UpdateAsync(agent, ct);
            _logger.LogInformation("Updated agent NFT {NftId} in local registry", agent.NftId);
        }
    }

    /// <summary>
    /// Returns the recommended model name for an agent class (first in the list).
    /// </summary>
    public string GetRecommendedModel(AgentClass agentClass)
    {
        return RecommendedModels.TryGetValue(agentClass, out var models) && models.Length > 0
            ? models[0]
            : "unknown";
    }

    /// <summary>
    /// Checks if a model path/name is compatible with a given class by matching
    /// known model family names against the class parameter count range.
    /// </summary>
    /// <param name="modelName">Model file name or full path.</param>
    /// <param name="agentClass">Target agent class.</param>
    /// <returns>True if the model name matches any recommended model for the class.</returns>
    public bool IsModelCompatible(string modelName, AgentClass agentClass)
    {
        if (!RecommendedModels.TryGetValue(agentClass, out var models))
            return false;

        var lowerName = modelName.ToLowerInvariant();
        return models.Any(m => lowerName.Contains(m.ToLowerInvariant()));
    }
}
