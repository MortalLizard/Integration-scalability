using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Database.Entities;

[Index(nameof(Isbn), IsUnique = true)]
public class Book
{
    public Book()
    {

    }

    [Key]
    public Guid Id { get; init; } = Guid.NewGuid();

    [Required]
    [MaxLength(20)]
    public required string Isbn { get; init; }

    [Required]
    [MaxLength(256)]
    public required string Title { get; init; }

    [Required]
    [MaxLength(256)]
    public required string Author { get; init; }

    [Required]
    [Column(TypeName = "text")]
    public required string Description { get; init; }

    [Required]
    public DateOnly PublishedDate { get; init; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "1000000")]
    [Column(TypeName = "decimal(18,4)")]
    public required decimal Price { get; init; }

    [Required]
    public DateTime CreatedAt { get; init; }

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public byte[]? RowVersion { get; set; }
}
