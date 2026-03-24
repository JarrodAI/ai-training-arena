using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AITrainingArena.Infrastructure.Data;

/// <summary>
/// SQLite-backed repository for battle entities.
/// Implements IBattleRepository (domain port).
/// </summary>
public class BattleRepository : IBattleRepository
{
    private readonly ArenaDbContext _db;

    public BattleRepository(ArenaDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<Battle?> GetByIdAsync(Guid battleId, CancellationToken ct = default)
        => await _db.Battles
            .Include(b => b.Rounds)
            .FirstOrDefaultAsync(b => b.BattleId == battleId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Battle>> GetByAgentAsync(
        string agentPeerId,
        int limit = 50,
        CancellationToken ct = default)
    {
        return await _db.Battles
            .Include(b => b.Rounds)
            .Where(b => b.ProposerId == agentPeerId || b.SolverId == agentPeerId)
            .OrderByDescending(b => b.StartedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Battle>> GetByStatusAsync(
        BattleStatus status,
        CancellationToken ct = default)
    {
        return await _db.Battles
            .Include(b => b.Rounds)
            .Where(b => b.Status == status)
            .OrderByDescending(b => b.StartedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task SaveAsync(Battle battle, CancellationToken ct = default)
    {
        var existing = await _db.Battles.FindAsync([battle.BattleId], ct);
        if (existing is null)
            _db.Battles.Add(battle);
        else
            _db.Entry(existing).CurrentValues.SetValues(battle);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<Battle>> GetRecentAsync(int count, CancellationToken ct = default)
    {
        return await _db.Battles
            .Include(b => b.Rounds)
            .OrderByDescending(b => b.StartedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Battle battle, CancellationToken ct = default)
    {
        _db.Battles.Add(battle);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Battle battle, CancellationToken ct = default)
    {
        _db.Battles.Update(battle);
        await _db.SaveChangesAsync(ct);
    }
}
