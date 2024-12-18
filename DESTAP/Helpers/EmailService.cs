using Microsoft.AspNetCore.Mvc;
using DESTAP.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace DESTAP.Helpers
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.office365.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUsername = "emutabakat@kansuk.com";
        private readonly string _smtpPassword = "9XzQXShm5..";
        
        public async Task<(bool Success, string ErrorMessage)> SendEmailAsync(MailModel mailModel)
        {
            try
            {
                using (var smtpClient = new SmtpClient(_smtpServer))
                {
                    smtpClient.Port = _smtpPort;
                    smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(mailModel.From ?? _smtpUsername),
                        Subject = mailModel.Subject,
                        Body = mailModel.Body,
                        IsBodyHtml = true // E-posta içeriği HTML ise true yapabilirsiniz.
                    };

                    // To ve CC alanlarını düzenlemek için yardımcı fonksiyon
                    string toEmails = CleanEmailAddresses(mailModel.To);
                    string ccEmails = CleanEmailAddresses(mailModel.Cc);
                    string bccEmails = CleanEmailAddresses(mailModel.Bcc);

                    // E-posta adreslerini ekle
                    if (!string.IsNullOrEmpty(toEmails))
                    {
                        mailMessage.To.Add(toEmails);
                    }

                    // CC ve BCC eklemeleri
                    if (!string.IsNullOrEmpty(ccEmails))
                    {
                        foreach (var cc in ccEmails.Split(','))
                        {
                            mailMessage.CC.Add(cc.Trim());
                        }
                    }

                    if (!string.IsNullOrEmpty(bccEmails))
                    {
                        foreach (var bcc in bccEmails.Split(','))
                        {
                            mailMessage.Bcc.Add(bcc.Trim());
                        }
                    }

                    // Dosya eklerini ekle
                    foreach (var attachment in mailModel.Attachments)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await attachment.CopyToAsync(memoryStream);
                            var attachmentContent = new Attachment(new MemoryStream(memoryStream.ToArray()), attachment.FileName);
                            mailMessage.Attachments.Add(attachmentContent);
                        }
                    }
                     
                    if (mailModel.BodyTableItems != null)
                    {
                        mailMessage.Body = BuildEmailBody(mailModel.BodyTableItems, mailModel.whoCreateRecord, mailModel.Subject);
                    }
                    // Eğer BodyTableItems nesnesi varsa
                    // E-posta gönder
                    await smtpClient.SendMailAsync(mailMessage);
                    return (true, null);
                }
            }
            catch (Exception e) 
            {
                // Hata durumunda loglama yapabilir ve false dönebilirsiniz.
                return (false, e.Message);
            }
        }
        private string CleanEmailAddresses(string emailAddresses)
        {
            if (string.IsNullOrEmpty(emailAddresses)) return string.Empty;

            // E-posta adreslerini virgülle ayır ve boş olanları filtrele
            var emailList = emailAddresses.Split(',')
                                           .Where(email => !string.IsNullOrEmpty(email?.Trim()))
                                           .Select(email => email.Trim())
                                           .ToList();

            // E-posta adreslerini virgülle birleştir
            return string.Join(", ", emailList);
        }
        
      
        // Mail içeriğini oluşturacak metod
        public string BuildEmailBody(List<string> record, string whoCreateRecord, string title)
        {
            return $@"
                            <div style='font-family: Arial, sans-serif; color: #333;'>
                                <table style='width: 100%; border: 0; cellpadding: 10; cellspacing: 0;'>
                                    <tr>
                                        <td style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                                            <h3 style='color: #007bff;'>{title}</h3>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                {BuildRecordDetailsTable(record)}
                </td>
                                    </tr>
                                    <tr style='text-align: center;'> 
                                        <td>
                                           Kaydı Oluşturan Kişi: <b> {whoCreateRecord}</b>
                                        </td>
                                        
                                    </tr>
                                    <tr style='text-align: center;'> 
                                        <td>
                                                Bu bir bilgi mailidir.
                                            </td>
                                    </tr>
                                    <tr>
                                        <td style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                                            <img src='http://www.kansuk.com/logo.png' alt='Company Logo' style='width:150px; height:auto;' />
                                        </td>
                                        
                                    </tr>
                                </table>
                            </div>
    ";
        }

        // Kayıt detayları için HTML tablosu oluşturan metod
        public string BuildRecordDetailsTable(List<string> record)
        {
            return $@"
            <table style='width: 100%; border-collapse: collapse;'>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>ID:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[0]}</td>
                                                </tr>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>Yıl:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[1]}</td>
                                                </tr>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>Sapma No:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[2]}</td>
                                                </tr>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>Sapma Açılış Tarihi:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[3]}</td>
                                                </tr>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>Açıklama:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[4]}</td>
                                                </tr>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>Aksiyon No:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[5]}</td>
                                                </tr>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>Aksiyon Açıklaması:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[6]}</td>
                                                </tr>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>Sorumlu Departman:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[7]}</td>
                                                </tr>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>Sorumlu Kişi:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[8]}</td>
                                                </tr>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>Durum:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[9]}</td>
                                                </tr>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>Hedef Tarih:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[10]}</td>
                                                </tr>
                                                <tr>
                                                    <td style='width: 40%; padding: 10px 0;'><strong>Tamamlanma Tarihi:</strong></td>
                                                    <td style='padding: 10px 0;'>{record[11]}</td>
                                                </tr>
                                            </table>
      
    ";
        }
    }
}
