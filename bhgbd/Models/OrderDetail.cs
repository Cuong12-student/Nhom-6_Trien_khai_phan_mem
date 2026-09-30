using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace bhgbd.Models
{
    public class OrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int odId { get; set; }
        [Required]
        public int quantity { get; set; }
        [Column(TypeName="decimal(18,2)")]
        public decimal price { get; set; }
        public int orderId { get; set; }
        [ForeignKey("orderId")]
        public Order? Order { get; set; }
        public int variantId { get; set; }
        [ForeignKey("variantId")]
        public ProductVariant? ProductVariant { get; set; }

    }
}
