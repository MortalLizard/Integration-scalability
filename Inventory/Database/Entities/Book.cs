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
    public required string Isbn { get; set; }

    [Required]
    [MaxLength(256)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(256)]
    public required string Author { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public required string Description { get; set; }

    [Required]
    public DateOnly PublishedDate { get; set; }

    [Required]
    [Range(typeof(int), "0", "1000000")]
    public int Quantity { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "1000000")]
    [Column(TypeName = "decimal(18,2)")]
    public required decimal Price { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }

    [Required]
    public required DateTime UpdatedAt { get; set; }
}
