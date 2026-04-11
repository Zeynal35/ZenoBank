using Microsoft.EntityFrameworkCore;
using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Account.Domain.Entities;

namespace ZenoBank.Services.Account.Infrastructure.Persistence;

public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(x => x.Balance)
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.RowVersion)
                .IsRowVersion();

            entity.HasIndex(x => x.AccountNumber)
                .IsUnique();

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.CustomerProfileId);
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
