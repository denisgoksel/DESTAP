using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DESTAP.Models
{
    [Table("TB_DVTrack")]
    public class DVTrackModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; } // Kimlik sütunu

        public int? UserID { get; set; } // Kullanıcı kimliği
        [ForeignKey("UserID")]
        public virtual UserModel User { get; set; } // UserID ile ilişkilendirilmiş kullanıcı
 
        public string CreatedYear { get; set; } // Oluşturulma yılı
         
        public string DeviationNo { get; set; } // Sapma numarası

        public DateTime? DeviationOpenDate { get; set; } // Sapma açılış tarihi

        public string DeviationDescription { get; set; } // Sapma açıklaması
 
        public string ActionNo { get; set; } // Aksiyon numarası

        public string ActionDescription { get; set; } // Aksiyon açıklaması
 
        public string RSP_Department { get; set; } // İlgili bölüm

       
        public int? RSP_User { get; set; } // İlgili kullanıcı ID'si
        [ForeignKey("RSP_User")]
        public virtual UserModel ResponsibleUser { get; set; } // RSP_User ile ilişkilendirilmiş kullanıcı
        public string? Other_RSPs { get; set; }
 
        public string State { get; set; } // Durum

        public DateTime? TargetDate { get; set; } // Hedef tarih
        public DateTime? CompleteDate { get; set; }

        public string Description { get; set; } // Açıklama

        public string FilePath { get; set; }

        //public DateTime? RecCreateAt { get; set; } // Kayıt oluşturma tarihi

        //public DateTime? RecUpdateAt { get; set; } // Kayıt güncelleme tarihi
    }
}
