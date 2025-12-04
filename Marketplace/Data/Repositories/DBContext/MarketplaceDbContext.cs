using System;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data.Entities;
using Marketplace.Data.Repositories;

namespace Marketplace.Data.Repositories.DBContext;

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

            entity.Property(b => b.ISBN)
                .HasMaxLength(50);

            entity.Property(b => b.Price)
                .HasPrecision(18, 2);

            entity.Property(b => b.PublishedDate);

            entity.Property(b => b.Description);

            entity.Property(b => b.CreatedAt)
                .IsRequired();

            entity.Property(b => b.UpdatedAt);

            entity.Property(b => b.IsActive);
        });
    }
}

