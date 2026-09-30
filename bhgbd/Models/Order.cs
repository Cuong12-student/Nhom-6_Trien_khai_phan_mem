using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
namespace bhgbd.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int orderId { get; set; }
        [Required]
        public DateTime orderDate { get; set; } = DateTime.Now;
        [Column(TypeName="decimal(18,2)")]
        public decimal total {  get; set; }
        [Required]
        public string receiverName { get; set; }
        [Required]
        public string receiverPhone { get; set; }
        [Required]
        public string receiverAddress { get; set; }
        [Required]
        public OrderStatus status { get; set; }
        public int customerId { get; set; }
        [ForeignKey("customerId")]
        public Customer? Customer { get; set; }
        public int? staffId { get; set; }
        [ForeignKey("staffId")]
        public Staff? Staff { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
    public enum OrderStatus
    {
        Pending,          // 0: Chờ duyệt (Trạng thái mặc định khi khách mới đặt)
        Confirmed,        // 1: Đã duyệt (Nhân viên đã xác nhận)
        CancelRequested,  // 2: Yêu cầu hủy (Khách bấm hủy đơn)
        Cancelled         // 3: Đã hủy (Nhân viên chốt hủy -> Kích hoạt code hoàn kho)
    }
}
