using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DESTAP.Models
{
    [Table("TB_Users")]  // Veritabanı tablosuyla ilişkilendiriliyor
    public class UserModel
    {
        [Key] // Birincil anahtar
        public int ID { get; set; }

        [Required] // Zorunlu alan
        [StringLength(100)] // Maksimum 100 karakter
        public string NameSurname { get; set; }

        [Required] // Zorunlu alan
        [StringLength(150)] // Maksimum 150 karakter
        public string Mail { get; set; }

        [Required] // Zorunlu alan
        public string Password { get; set; }

        [StringLength(100)] // Maksimum 100 karakter
        public string Department { get; set; }

        [StringLength(150)] // Maksimum 150 karakter
        public string ManagerMail { get; set; }
        public DateTime CreateDate { get; set; }

        // Foreign Key (Yabancı Anahtar)
        public int Role { get; set; }

        // Navigation Property (İlişkiyi temsil eder)
        [ForeignKey("Role")]
        public virtual RoleModel RoleDetails { get; set; } // RoleId'ye karşılık gelir
        public virtual ICollection<ChangeTrackModel> CHTracks { get; set; } // Kullanıcıya ait sapmalar
        public virtual ICollection<ChangeTrackModel> ResponsibleCHTracks { get; set; } // Sorumlu olduğu sapmalar
        public virtual ICollection<DVTrackModel> DVTracks { get; set; } // Kullanıcıya ait sapmalar
        public virtual ICollection<DVTrackModel> ResponsibleDVTracks { get; set; } // Sorumlu olduğu sapmalar
        public virtual ICollection<CPATrackModel> CPATracks { get; set; } // Kullanıcıya ait sapmalar
        public virtual ICollection<CPATrackModel> ResponsibleCPATracks { get; set; } // Sorumlu olduğu sapmalar

    }
}
