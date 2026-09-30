using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace bhgbd.Models
{
    [Table("products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int productId { get; set; }
        [Required]
        public string productName { get; set; }
        [Required]
        public string description { get; set; }
        [Column(TypeName =("decimal(18,2)"))]
        public decimal price { get; set; }
        public string? imageUrl { get; set; } = "/images/products/default.webp";
        public DateTime createdAt { get; set; }= DateTime.Now;
        public int categoryId { get; set; }
        [ForeignKey("categoryId")]
        public Category? Category { get; set; }
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    }
}
