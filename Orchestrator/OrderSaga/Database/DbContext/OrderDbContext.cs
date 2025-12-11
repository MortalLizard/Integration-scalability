using Microsoft.EntityFrameworkCore;

namespace Orchestrator.OrderSaga.Database.DbContext;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
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
                .HasMaxLength(320); // enough for email

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
