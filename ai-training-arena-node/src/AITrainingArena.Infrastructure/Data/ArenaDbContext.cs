using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AITrainingArena.Infrastructure.Data;

public class ArenaDbContext : DbContext
{
    public ArenaDbContext(DbContextOptions<ArenaDbContext> options) : base(options) { }

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Battle> Battles => Set<Battle>();
    public DbSet<Question> Questions => Set<Question>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasKey(a => a.NftId);
            entity.Property(a => a.NftId).ValueGeneratedNever();
            entity.Property(a => a.ModelName).HasMaxLength(256);
            entity.Property(a => a.EloRating)
                .HasConversion(r => (int)r, v => new EloRating(v));
            entity.Property(a => a.OwnerAddress)
                .HasMaxLength(256)
                .HasConversion(w => (string)w, v => new WalletAddress(v));
            entity.Property(a => a.StakedAmount).HasColumnType("TEXT");
            entity.Ignore(a => a.DomainEvents);
        });

        modelBuilder.Entity<Battle>(entity =>
        {
            entity.HasKey(b => b.BattleId);
            entity.Property(b => b.ProposerId).HasMaxLength(256);
            entity.Property(b => b.SolverId).HasMaxLength(256);
            entity.OwnsOne(b => b.FinalResult);
            entity.HasMany(b => b.Rounds).WithOne();
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(q => q.Text);
            entity.Property(q => q.Text).HasMaxLength(4096);
            entity.Property(q => q.ExpectedAnswer).HasMaxLength(4096);
            entity.Property(q => q.DifficultyScore).HasColumnType("TEXT");
        });
    }
}
