using Microsoft.EntityFrameworkCore;
using Marketplace.Database.Entities;

namespace Marketplace.Database.DBContext;

public class MarketplaceDbContext : DbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(b => b.Author)
                .HasMaxLength(256);

            entity.Property(b => b.Isbn)
                .HasMaxLength(50);

            entity.Property(b => b.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(b => b.PublishedDate);

            entity.Property(b => b.Description);

            entity.Property(b => b.CreatedAt)
                .IsRequired();

            entity.Property(b => b.UpdatedAt)
                .IsRequired();

            entity.Property(b => b.IsActive)
                .IsRequired();
        });
    }
}

