using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DESTAP.Models
{
    public class MailModel
    {
        public string To { get; set; }          // Alıcı e-posta adresi
        public string Subject { get; set; }     // E-posta konusu
        public string? Body { get; set; }    
        public List<string>? BodyTableItems { get; set; }        // E-posta içeriği
        public string From { get; set; }        // Gönderen e-posta adresi (isteğe bağlı)
        public string Cc { get; set; }    // Bilgilerinizi paylaşmak isteyen kişi veya kişilere CC
        public string Bcc { get; set; }   // Gizli alıcılar (isteğe bağlı)
        public string whoCreateRecord { get; set; }    
        // E-posta dosya ekleri için isteğe bağlı bir liste
        public List<IFormFile> Attachments { get; set; }

        public MailModel()
        {
            Cc = string.Empty;
            Bcc = string.Empty;
            Attachments = new List<IFormFile>();
        }
    }
    
   
    
}
