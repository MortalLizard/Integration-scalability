using Microsoft.EntityFrameworkCore;
using Orchestrator.OrderSaga.Database.Entities;

namespace Orchestrator.OrderSaga.Database.DbContext;

public sealed class OrderDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderSagaState> OrderSagas => Set<OrderSagaState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderSagaState>(e =>
        {
            e.ToTable("OrderSagas");

            e.HasKey(x => x.OrderId);

            e.Property(x => x.BuyerEmail)
                .IsRequired()
                .HasMaxLength(256);

            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            e.Property(x => x.FailureReason)
                .HasMaxLength(2000);

            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime2");

            e.Property(x => x.UpdatedAt)
                .HasColumnType("datetime2");

        });
    }
}
