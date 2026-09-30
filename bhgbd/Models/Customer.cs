using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace bhgbd.Models
{
    [Table("customers")]
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int customerId { get; set; }
        [Required, StringLength(50)]
        public string customerName { get; set; }
        [Required]
        public int age { get; set; }
        [Required, StringLength(3)]
        public Gender gender { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public string phone { get; set; }
        public int userId { get; set; }
        [ForeignKey("userId")]
        public User? User {  get; set; }
    }
}
