using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DESTAP.Models
{
    [Table("TB_ChangeTrack")]
    public class ChangeTrackModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; } // Identity kolonu
        public int? UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual UserModel User { get; set; } // UserID ile ilişkilendirilmiş kullanıcı
        public string CreatedYear { get; set; }
        public string ChangeNo { get; set; }
        public DateTime? ChangeOpenDate { get; set; }
        public string ChangeDescription { get; set; }
        public string ActionNo { get; set; }
        public string ActionDescription { get; set; }
        public string RSP_Department { get; set; }
        public int? RSP_User { get; set; }
        [ForeignKey("RSP_User")]
        public virtual UserModel ResponsibleUser { get; set; } // RSP_User ile ilişkilendirilmiş kullanıcı
        public string? Other_RSPs { get; set; }
        public string State { get; set; }
        public DateTime? TargetDate1 { get; set; }
        public DateTime? TargetDate2 { get; set; }
        public DateTime? TargetDate3 { get; set; }
        public DateTime? CompleteDate { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        //public DateTime? RecCreateAt { get; set; }
        //public DateTime? RecUpdateAt { get; set; }
    }
}
