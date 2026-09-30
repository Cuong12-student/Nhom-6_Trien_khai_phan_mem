using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace bhgbd.Models
{
    [Table("productvariants")]
    public class ProductVariant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public int size { get; set; }
        [Required]
        [StringLength(50)]
        public string soleType { get; set; }
        [Required]
        public int quantity { get; set; }
        public int productId { get; set; }
        [ForeignKey("productId")]
        public Product? Product { get; set; }
    }
}
