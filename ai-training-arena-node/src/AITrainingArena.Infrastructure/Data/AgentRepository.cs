using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AITrainingArena.Infrastructure.Data;

/// <summary>
/// SQLite-backed repository for agent entities.
/// Implements IAgentRepository (domain port).
/// </summary>
public class AgentRepository : IAgentRepository
{
    private readonly ArenaDbContext _db;

    public AgentRepository(ArenaDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<Agent?> GetByNftIdAsync(uint nftId, CancellationToken ct = default)
        => await _db.Agents.FirstOrDefaultAsync(a => a.NftId == nftId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Agent>> GetByOwnerAsync(string ownerAddress, CancellationToken ct = default)
    {
        return await _db.Agents
            .Where(a => a.OwnerAddress == ownerAddress)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Agent>> GetLeaderboardAsync(
        AgentClass agentClass,
        int topN = 100,
        CancellationToken ct = default)
    {
        return await _db.Agents
            .Where(a => a.Class == agentClass && a.IsActive)
            .OrderByDescending(a => (int)a.EloRating)
            .Take(topN)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task SaveAsync(Agent agent, CancellationToken ct = default)
    {
        var existing = await _db.Agents.FindAsync([agent.NftId], ct);
        if (existing is null)
            _db.Agents.Add(agent);
        else
            _db.Entry(existing).CurrentValues.SetValues(agent);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddAsync(Agent agent, CancellationToken ct = default)
    {
        _db.Agents.Add(agent);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Agent agent, CancellationToken ct = default)
    {
        _db.Agents.Update(agent);
        await _db.SaveChangesAsync(ct);
    }
}
