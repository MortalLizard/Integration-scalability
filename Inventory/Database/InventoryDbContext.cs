using Inventory.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Database;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(b =>
        {
            b.HasIndex(x => x.Isbn).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }

}
