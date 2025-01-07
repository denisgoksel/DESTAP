using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DESTAP.Models
{
    [Table("TB_CPATrack")]
    public class CPATrackModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int? UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual UserModel User { get; set; } // UserID ile ilişkilendirilmiş kullanıcı
 
        public string CreatedYear { get; set; }
 
        public string CPANo { get; set; }

        public DateTime? CPAOpenDate { get; set; }

        public string CPADescription { get; set; }

        public string ActionNo { get; set; }

        public string ActionDescription { get; set; }

        [MaxLength(50)]
        public string RSP_Department { get; set; }
                
        public int? RSP_User { get; set; }
        [ForeignKey("RSP_User")]
        public virtual UserModel ResponsibleUser { get; set; } // RSP_User ile ilişkilendirilmiş kullanıcı
        /*private string? _otherRSPs;
         * 'System.NullReferenceException' hatasından kaçınma
        public string? Other_RSPs
        {
            get => _otherRSPs ?? "";  // Eğer _otherRSPs null ise boş string döndür
            set => _otherRSPs = value;  // null değer gelebilir, onu _otherRSPs olarak ayarlıyoruz
        }*/
        public string? Other_RSPs { get; set; }
         
        public string State { get; set; }

        public DateTime? TargetDate { get; set; }

        public DateTime? CompleteDate { get; set; }

        public string Description { get; set; }
        public string FilePath { get; set; }

        //public DateTime? RecCreateAt { get; set; }

        //public DateTime? RecUpdateAt { get; set; }
    }
}
