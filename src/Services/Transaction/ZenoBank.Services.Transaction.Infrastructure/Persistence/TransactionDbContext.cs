using Microsoft.EntityFrameworkCore;
using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Transaction.Domain.Entities;

namespace ZenoBank.Services.Transaction.Infrastructure.Persistence;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
    {
    }

    public DbSet<TransactionRecord> Transactions => Set<TransactionRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TransactionRecord>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ReferenceNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            entity.HasIndex(x => x.ReferenceNumber).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.FromAccountId);
            entity.HasIndex(x => x.ToAccountId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Action).IsRequired().HasMaxLength(100);
            entity.Property(x => x.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(x => x.EntityId).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(1000);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(50);

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.CreatedAtUtc);
        });
    }
}