using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace bhgbd.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int categoryId { get; set; }
        [Required]
        public string categoryName { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public bool isActive { get; set; }
    }
}
