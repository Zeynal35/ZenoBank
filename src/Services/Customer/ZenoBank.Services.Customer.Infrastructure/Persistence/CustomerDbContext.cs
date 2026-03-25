using Microsoft.EntityFrameworkCore;
using ZenoBank.Services.Customer.Domain.Entities;

namespace ZenoBank.Services.Customer.Infrastructure.Persistence;

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomerProfile>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(x => x.Address)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.Status)
                .IsRequired();

            entity.HasIndex(x => x.UserId)
                .IsUnique();
        });
    }
}
