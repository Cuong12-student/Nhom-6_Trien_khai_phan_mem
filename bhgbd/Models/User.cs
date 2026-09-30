using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace bhgbd.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int userId { get; set; }
        [Required, StringLength(50)]
        public string username { get; set; }
        [Required,StringLength(50)]
        public string password { get; set; }
        [Required]
        public UserRole role { get; set; }
    }
    public enum UserRole
    {
        Admin,
        Staff,
        Customer
    }
}
