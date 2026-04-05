using Microsoft.EntityFrameworkCore;
using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Customer.Domain.Entities;

namespace ZenoBank.Services.Customer.Infrastructure.Persistence;

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<KycDocument> KycDocuments => Set<KycDocument>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomerProfile>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(30);
            entity.Property(x => x.Address).IsRequired().HasMaxLength(500);
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.BlacklistReason).HasMaxLength(500);
            entity.Property(x => x.RiskLevel).IsRequired();

            entity.HasIndex(x => x.UserId).IsUnique();
        });

        modelBuilder.Entity<KycDocument>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DocumentNumber).IsRequired().HasMaxLength(100);
            entity.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(x => x.StoredFileName).IsRequired().HasMaxLength(255);
            entity.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(x => x.ReviewerNote).HasMaxLength(1000);

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
