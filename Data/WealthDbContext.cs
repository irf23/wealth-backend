using Microsoft.EntityFrameworkCore;
using WealthBackend.Models;

namespace WealthBackend.Data
{
    public class WealthDbContext : DbContext
    {
        public WealthDbContext(DbContextOptions<WealthDbContext> options)
            : base(options)
        {
        }

        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<AssetBalanceHistory> AssetBalanceHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Asset entity
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.ToTable("Assets");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.AssetName)
                    .HasMaxLength(500);

                entity.Property(e => e.PrimaryAssetCategory)
                    .HasMaxLength(100);

                entity.Property(e => e.WealthAssetType)
                    .HasMaxLength(100);

                entity.Property(e => e.BalanceCurrent)
                    .HasPrecision(18, 2);

                entity.HasIndex(e => e.PrimaryAssetCategory);
                entity.HasIndex(e => e.WealthAssetType);
                entity.HasIndex(e => e.BalanceAsOf);
            });

            // Configure AssetBalanceHistory entity
            modelBuilder.Entity<AssetBalanceHistory>(entity =>
            {
                entity.ToTable("AssetBalanceHistories");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Balance)
                    .HasPrecision(18, 2);

                // Configure relationship
                entity.HasOne(e => e.Asset)
                    .WithMany(a => a.BalanceHistory)
                    .HasForeignKey(e => e.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Create composite index for efficient point-in-time queries
                entity.HasIndex(e => new { e.AssetId, e.BalanceAsOf })
                    .HasDatabaseName("IX_AssetBalanceHistory_AssetId_BalanceAsOf");

                entity.HasIndex(e => e.BalanceAsOf);
            });
        }
    }
}
