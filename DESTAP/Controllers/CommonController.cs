using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DESTAP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DESTAP.Helpers
{
    [Authorize]
    public class CommonController : Controller
    {
        private readonly DB_Context _context;
        private readonly EmailService _emailService;

        public CommonController(DB_Context context)
        {
            _context = context;
            _emailService = new EmailService();
        }
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.TB_Users
                                   .Select(u => new { u.ID, u.NameSurname })
                                   .ToListAsync();

                return Json(new { success = true, users = users });
            }
            catch (Exception ex)
            {
                // Konsola hata yazdırma
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Internal Server Error: " + ex.Message });
            }

        }
        public async Task<JsonResult> GetUserList(string userIDs)
        {
            try
            {
                string[] IDs = userIDs.Split(',');
                List<string> Users = new List<string>();

                foreach (string item in IDs)
                {
                    var userEntity = await _context.TB_Users
                                .Where(x => x.ID == Convert.ToInt32(item))
                                .FirstOrDefaultAsync();

                    string user = userEntity?.NameSurname ?? "Kullanıcı Bulunamadı";
                    Users.Add(user);
                }

                return Json(new { success = true, users = Users });
            }
            catch (Exception ex)
            {
                // Konsola hata yazdırma
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Internal Server Error: " + ex.Message });
            }

        }
        // Departmanı seçtiğimizde kullanıcıları getirecek bir action
        [HttpPost]
        public async Task<IActionResult> GetUsersByDepartment(string department)
        {
            try
            {
                if (string.IsNullOrEmpty(department))
                {
                    return Json(new { success = false, message = "Department cannot be empty" });
                }

                var users = await _context.GetUsersByDepartmentAsync(department);

                return Json(new { success = true, users = users });
            }
            catch (Exception ex)
            {
                // Konsola hata yazdırma
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Internal Server Error: " + ex.Message });
            }
        }
        public async Task<IActionResult> SendEmail(string userID, int recordId, string subject, string tableType)
        {
            // Kayıt verisini al
            object record = null;
            List<string> BodyTableItems = new List<string>();

            switch (tableType)
            {
                case "CPATrack":
                    record = await _context.TB_CPATrack
                                            .Include(x => x.ResponsibleUser)
                                            .FirstOrDefaultAsync(x => x.ID == recordId);
                    if (record != null)
                    {
                        var cpaRecord = (CPATrackModel)record;
                        BodyTableItems = new List<string>
                {
                    cpaRecord.ID.ToString(),
                    cpaRecord.CreatedYear.ToString(),
                    cpaRecord.CPANo ?? string.Empty,
                    cpaRecord.CPAOpenDate?.ToString("dd.MM.yyyy") ?? string.Empty,
                    cpaRecord.CPADescription ?? string.Empty,
                    cpaRecord.ActionNo ?? string.Empty,
                    cpaRecord.ActionDescription ?? string.Empty,
                    cpaRecord.RSP_Department ?? string.Empty,
                    cpaRecord.ResponsibleUser?.NameSurname ?? string.Empty,
                    cpaRecord.State ?? string.Empty,
                    cpaRecord.TargetDate?.ToString("dd.MM.yyyy") ?? string.Empty,
                    cpaRecord.CompleteDate?.ToString("dd.MM.yyyy") ?? string.Empty
                };
                    }
                    break;

                case "CHTrack":
                    record = await _context.TB_ChangeTrack
                                            .Include(x => x.ResponsibleUser)
                                            .FirstOrDefaultAsync(x => x.ID == recordId);
                    if (record != null)
                    {
                        var changeRecord = (ChangeTrackModel)record;
                        BodyTableItems = new List<string>
                {
                    changeRecord.ID.ToString(),
                    changeRecord.CreatedYear.ToString(),
                    changeRecord.ChangeNo ?? string.Empty,
                    changeRecord.ChangeOpenDate?.ToString("dd.MM.yyyy") ?? string.Empty,
                    changeRecord.ChangeDescription ?? string.Empty,
                    changeRecord.ActionNo ?? string.Empty,
                    changeRecord.ActionDescription ?? string.Empty,
                    changeRecord.RSP_Department ?? string.Empty,
                    changeRecord.ResponsibleUser?.NameSurname ?? string.Empty,
                    changeRecord.State ?? string.Empty,
                    changeRecord.TargetDate1?.ToString("dd.MM.yyyy") ?? string.Empty,
                    changeRecord.CompleteDate?.ToString("dd.MM.yyyy") ?? string.Empty,
                    changeRecord.TargetDate2?.ToString("dd.MM.yyyy") ?? string.Empty,
                    changeRecord.TargetDate3?.ToString("dd.MM.yyyy") ?? string.Empty
                };
                    }
                    break;

                case "DVTrack":
                    record = await _context.TB_DVTrack
                                            .Include(x => x.ResponsibleUser)
                                            .FirstOrDefaultAsync(x => x.ID == recordId);
                    if (record != null)
                    {
                        var dvRecord = (DVTrackModel)record;
                        BodyTableItems = new List<string>
                {
                    dvRecord.ID.ToString(),
                    dvRecord.CreatedYear.ToString(),
                    dvRecord.DeviationNo ?? string.Empty,
                    dvRecord.DeviationOpenDate?.ToString("dd.MM.yyyy") ?? string.Empty,
                    dvRecord.DeviationDescription ?? string.Empty,
                    dvRecord.ActionNo ?? string.Empty,
                    dvRecord.ActionDescription ?? string.Empty,
                    dvRecord.RSP_Department ?? string.Empty,
                    dvRecord.ResponsibleUser?.NameSurname ?? string.Empty,
                    dvRecord.State ?? string.Empty,
                    dvRecord.TargetDate?.ToString("dd.MM.yyyy") ?? string.Empty,
                    dvRecord.CompleteDate?.ToString("dd.MM.yyyy") ?? string.Empty
                };
                    }
                    break;

                default:
                    return Json(new { success = false, message = "Geçersiz tablo tipi." });
            }

            if (record == null)
            {
                return Json(new { success = false, message = "Kayıt bulunamadı." });
            }

            // Mail modelini oluştur
            var mailModel = await CreateMailModel(userID, recordId, "KALİTE GÜVENCE", subject, null, BodyTableItems, tableType);

            if (mailModel == null)
            {
                return Json(new { success = false, message = "E-posta oluşturulurken bir hata oluştu." });
            }

            // E-posta gönderimi
            /* AÇILACAK --------     var emailSent = await _emailService.SendEmailAsync(mailModel);

            return Json(new { success = emailSent.Success, message = emailSent.Success ? "E-posta başarıyla gönderildi." : "E-posta gönderilirken bir hata oluştu." });
            */
            return Json(new { success = "false", message = "E-posta gönderilirken bir hata oluştu." }); ///Silinecek -test MODE ON
        }
        public async Task<MailModel> CreateMailModel(string UserID, int RecordID, string department, string subject, string body, List<string> bodyTableItems, string tableType)
        {
            try
            {
                // Declare record as a dynamic object to hold different types
                object record = null;

                if (tableType == "CPATrack")
                {
                    record = await _context.TB_CPATrack
                                           .Include(r => r.ResponsibleUser)
                                           .FirstOrDefaultAsync(r => r.ID == RecordID);
                }
                else if (tableType == "CHTrack")
                {
                    record = await _context.TB_ChangeTrack
                                           .Include(r => r.ResponsibleUser)
                                           .FirstOrDefaultAsync(r => r.ID == RecordID);
                }
                else if (tableType == "DVTrack")
                {
                    record = await _context.TB_DVTrack
                                           .Include(r => r.ResponsibleUser)
                                           .FirstOrDefaultAsync(r => r.ID == RecordID);
                }

                if (record == null) return null;

                var qualityAssuranceUsers = await _context.TB_Users
                                                          .Where(u => u.Department == department)
                                                          .ToListAsync();
                var allUsers = await _context.TB_Users.ToListAsync();
                var whoChangedRec = await _context.TB_Users
                                                  .FirstOrDefaultAsync(u => u.ID.ToString() == UserID);

                // Determine the recipient email addresses
                string toMail = string.Empty;
                string ccMail = string.Empty;
                string other_RSPs_Mails = string.Empty;

                if (record is CPATrackModel cpaRecord)
                {
                    toMail = cpaRecord.ResponsibleUser?.Mail != null ? ", " + cpaRecord.ResponsibleUser.Mail : "";
                    ccMail = cpaRecord.ResponsibleUser?.ManagerMail ?? "";
                    string[] other_RSPs = cpaRecord.Other_RSPs?.Split(',').Select(id => id.Trim()).ToArray() ?? new string[] { };
                    other_RSPs_Mails = string.Join(", ", other_RSPs.Select(id => allUsers.FirstOrDefault(u => u.ID == int.Parse(id))?.Mail));
                }
                else if (record is ChangeTrackModel changeRecord)
                {
                    toMail = changeRecord.ResponsibleUser?.Mail != null ? ", " + changeRecord.ResponsibleUser.Mail : "";
                    ccMail = changeRecord.ResponsibleUser?.ManagerMail ?? "";
                    string[] other_RSPs = changeRecord.Other_RSPs?.Split(',').Select(id => id.Trim()).ToArray() ?? new string[] { };
                    other_RSPs_Mails = string.Join(", ", other_RSPs.Select(id => allUsers.FirstOrDefault(u => u.ID == int.Parse(id))?.Mail));
                }
                else if (record is DVTrackModel dvRecord)
                {
                    toMail = dvRecord.ResponsibleUser?.Mail != null ? ", " + dvRecord.ResponsibleUser.Mail : "";
                    ccMail = dvRecord.ResponsibleUser?.ManagerMail ?? "";
                    string[] other_RSPs = dvRecord.Other_RSPs?.Split(',').Select(id => id.Trim()).ToArray() ?? new string[] { };
                    other_RSPs_Mails = string.Join(", ", other_RSPs.Select(id => allUsers.FirstOrDefault(u => u.ID == int.Parse(id))?.Mail));
                }

                // Get QA emails
                string qaEmails = string.Join(", ", qualityAssuranceUsers.Select(u => u.Mail));

                var mailModel = new MailModel
                {
                    To = $"{toMail}, {other_RSPs_Mails}",
                    Cc = $"{ccMail}, {qaEmails}",
                    Subject = subject,
                    Body = body,
                    BodyTableItems = bodyTableItems,
                    whoCreateRecord = whoChangedRec.NameSurname,
                    From = "emutabakat@kansuk.com"
                };

                return mailModel;
            }
            catch (Exception)
            {
                return null;
            }
        }



        private async Task<List<string>> SaveFilesForModel<T>(T model, IEnumerable<IFormFile> files, string folderName, int recordId)
        {
            var savedFilePaths = new List<string>();

            if (files != null && files.Any())
            {
                // Base directory for uploads
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/uploads/{folderName}");
                Directory.CreateDirectory(uploadsFolder); // Create folder if it doesn't exist

                int fileCounter = 1; // Optional: Counter for file naming
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // File extension
                        string extension = Path.GetExtension(file.FileName);

                        // Unique file name: RecordId_FileCounter_Guid.extension
                        string uniqueFileName = $"{recordId}_{fileCounter}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Save the file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Add the relative file path to the result list
                        savedFilePaths.Add($"/uploads/{folderName}/{uniqueFileName}");
                        fileCounter++;
                    }
                }
            }

            return savedFilePaths; // Return all saved file paths
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<List<string>> CreateFiles(object model, List<IFormFile> fileUploads)
        {
             
                int recordId = 0;
                string folderName = "";

                // Model'e göre kayıt ID'si ve klasör adı belirleme
                switch (model)
                {
                    case ChangeTrackModel changeTrackModel:
                        //_context.TB_ChangeTrack.Add(changeTrackModel);
                        //await _context.SaveChangesAsync();
                        recordId = changeTrackModel.ID; // Kaydedilen kaydın ID'sini al
                        folderName = "ChangeTrack";
                        break;

                    case CPATrackModel cPtrackModel:
                        //_context.TB_CPATrack.Add(cPtrackModel);
                        //await _context.SaveChangesAsync();
                        recordId = cPtrackModel.ID;
                        folderName = "CPtrack";
                        break;

                    case DVTrackModel dvTrackModel:
                        //_context.TB_DVTrack.Add(dvTrackModel);
                        //await _context.SaveChangesAsync();
                        recordId = dvTrackModel.ID;
                        folderName = "DVTrack";
                        break;

                    default:
                        return null;
                }

                var filePaths = new List<string>();

                
                    if (fileUploads != null && fileUploads.Count > 0)
                    {
                        filePaths = await SaveFilesForModel(model, fileUploads, folderName, recordId);
                    }
                

                return filePaths;
               
          
             
        }


    }
}
