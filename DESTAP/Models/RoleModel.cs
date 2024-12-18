using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DESTAP.Models
{
    [Table("TB_Roles")]
    public class RoleModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int RoleId { get; set; }

        [Required]
        [StringLength(100)] // Maksimum 100 karakter
        public string RoleName { get; set; }

        [StringLength(250)] // Maksimum 250 karakter
        public string RoleDescription { get; set; }
    }
}