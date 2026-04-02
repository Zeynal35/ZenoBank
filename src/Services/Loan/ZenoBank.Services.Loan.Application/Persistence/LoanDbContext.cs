using Microsoft.EntityFrameworkCore;
using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Loan.Domain.Entities;

namespace ZenoBank.Services.Loan.Infrastructure.Persistence;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
    {
    }

    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrincipalAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.InterestRate).HasColumnType("decimal(18,2)");
            entity.Property(x => x.MonthlyPayment).HasColumnType("decimal(18,2)");
            entity.Property(x => x.TotalRepayment).HasColumnType("decimal(18,2)");

            entity.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(x => x.Purpose)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.RejectionReason)
                .HasMaxLength(500);

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