using DESTAP.Helpers;
using DESTAP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
 
using System.Security.Claims;
using System.Threading.Tasks;


// DVTrackController : Deviation action
// Sapma faaliyet
// updated:22.11.2024
// created by: Deniz Göksel

namespace DESTAP.Controllers
{
    public class DVTrackController : Controller
    {
        private readonly DB_Context _context;
        private readonly EmailService _emailService;
        private readonly CommonController _commonsController;

        // Constructor to inject dependencies
        public DVTrackController(DB_Context context, EmailService emailService, CommonController commonsController)
        {
            _context = context;
            _emailService = emailService;
            _commonsController = commonsController;
        }
        public IActionResult Index()
        {
            var records = _context.TB_DVTrack
                                   .Include(u=> u.ResponsibleUser)
                                    .ToList();  // Tüm kayıtları al
            

            return View(records);  // View'a gönder
        }


        // Display the Create form
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Departments'ı stored procedure ile alıyoruz
            var departments = await _context.GetDepartmentsFromProcedureAsync();

            // View'a veriyi gönderiyoruz
            ViewBag.Departments = new SelectList(departments, "Department", "Department");
            var model = new DVTrackModel();
            return View(model);
        }

        // Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DVTrackModel changeTrack)
        {
            if (ModelState.IsValid)
            {
                
                _context.TB_DVTrack.Add(changeTrack);
                await _context.SaveChangesAsync();
                 
                
                await _commonsController.SendEmail(User.FindFirst(ClaimTypes.NameIdentifier).Value, _context.TB_DVTrack.MaxAsync(u => u.ID).Result, "TEST MAİLİ SAPMA KAYDI OLUŞTURULDU", "DVTrack");
                return RedirectToAction("Index");
            }
            return View(changeTrack);
        }
        // GET: CHTrack/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var record = await _context.TB_DVTrack.FindAsync(id);
            var departments = await _context.GetDepartmentsFromProcedureAsync();
            ViewBag.Departments = new SelectList(departments, "Department", "Department");
            ViewBag.SelectedVals = !string.IsNullOrEmpty(record.Other_RSPs)
                                                                         ? record.Other_RSPs.Split(',').Select(id => id.Trim()).ToArray()
                                                                         : new string[0];
            var users = await _context.GetUsersByDepartmentAsync(record.RSP_Department.ToString());
            ViewBag.DefaultUser = _context.TB_Users.FirstOrDefaultAsync(u => u.ID == record.RSP_User).Result.NameSurname.ToString();
            ViewBag.SavedUser = new SelectList(users, "ID", "NameSurname");
            if (record == null)
            {
                return NotFound();
            }
            return View(record);
        }

        // POST: CHTrack/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DVTrackModel model)
        {
            if (id != model.ID)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    
                    await _commonsController.SendEmail(User.FindFirst(ClaimTypes.NameIdentifier).Value, id, "TEST MAİLİ SAPMA KAYDI GÜNCELLENDİ.", "DVTrack");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Veri güncellenirken bir hata oluştu.");
                }
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC BackupAndDeleteRecord_DV @RecordID = {0}", id);
            }
            catch (Exception e)
            {
                TempData["Hata"] = e.Message;
                throw;
            }
            
            return Ok();
        }

        [HttpPost]
        public async Task<JsonResult> SendFeedback(int UserID, int RecordID, string FeedbackDescription)
        {
            try
            {
                // Kayıt verisini al
                var record = await _context.TB_DVTrack
                                           .Include(r => r.ResponsibleUser)
                                           .FirstOrDefaultAsync(r => r.ID == RecordID);
                if (record == null)
                {
                    return Json(new { success = false, message = "Kayıt bulunamadı." });
                }

                // Mail içeriğini oluşturmak için CreateMailModel metodunu kullan
                var subject = "TEST MAIL - Sapma Yeni Geri Bildirim";
                var body = $"Geri Bildirim: {FeedbackDescription}";

                var mailModel = await _commonsController.CreateMailModel(User.FindFirst(ClaimTypes.NameIdentifier).Value, RecordID, "KALİTE GÜVENCE", subject, body, null, "DVTrack");

                if (mailModel == null)
                {
                    return Json(new { success = false, message = "Mail model oluşturulamadı." });
                }

                // Mail gönderimi
                var result = await _emailService.SendEmailAsync(mailModel);

                return Json(new { success = result.Success });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CompleteRecord(int RecordID, DateTime CompletionDate, string CompletionDescription)
        {
            try
            {
                // Kayıt verisini güncelle
                var record = await _context.TB_DVTrack
                                           .Include(r => r.ResponsibleUser)
                                           .FirstOrDefaultAsync(r => r.ID == RecordID);
                if (record == null)
                {
                    return Json(new { success = false, message = "Kayıt bulunamadı." });
                }

                // Kayıt tamamlandığında tarih ve açıklama bilgilerini kaydet
                record.CompleteDate = CompletionDate;
                record.Description = CompletionDescription;
                await _context.SaveChangesAsync();

                // Mail içeriğini oluşturmak için CreateMailModel metodunu kullan
                var subject = "TEST-MAIL Sapma Tamamlanmış Kayıt Bildirimi";
                var body = $"Kayıt ID: {RecordID}, Tamamlama Tarihi: {CompletionDate:yyyy.MM.dd}, Açıklama: {CompletionDescription}, <br> Kaydı Değiştiren: {User.Identity.Name}";
                var mailModel = await _commonsController.CreateMailModel(User.FindFirst(ClaimTypes.NameIdentifier).Value, RecordID, "KALİTE GÜVENCE", subject, body, null, "DVTrack");

                if (mailModel == null)
                {
                    return Json(new { success = false, message = "Mail model oluşturulamadı." });
                }

                // Mail gönderme işlemi
                var result = await _emailService.SendEmailAsync(mailModel);

                if (result.Success)
                {
                    return Json(new { success = true, completionDate = CompletionDate.ToString("yyyy.MM.dd") });
                }
                else
                {
                    return Json(new { success = false, message = "Mail gönderilemedi. Lütfen tekrar deneyin." });
                }
            }
            catch (DbUpdateException ex)
            {
                // Veritabanı hatası durumunda mesaj döndür
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = "Veritabanı güncelleme hatası: " + errorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> SubmitTargetDateChange(int recordId, string requestedDate, string changeDescription)
        {
            try
            {
                var record = await _context.TB_DVTrack
                                           .Include(r => r.ResponsibleUser)
                                           .FirstOrDefaultAsync(r => r.ID == recordId);

                if (record == null)
                {
                    return Json(new { success = false, message = "Kayıt bulunamadı." });
                }

                // E-posta içeriği ve konu oluşturuluyor
                var emailSubject = "TEST Mail - Sapma Hedef Tarihi Değişikliği Talebi";
                var emailBody = $"Kayıt No: {recordId}\nTalep Edilen Tarih: {requestedDate}\nAçıklama: {changeDescription}";

                // CreateMailModel metodunu kullanarak mail modelini oluşturuyoruz
                var mailModel = await _commonsController.CreateMailModel(User.FindFirst(ClaimTypes.NameIdentifier).Value, recordId, "KALİTE GÜVENCE", emailSubject, emailBody, null, "DVTrack");

                if (mailModel == null)
                {
                    return Json(new { success = false, message = "E-posta modeli oluşturulurken bir hata oluştu." });
                }

                // Mail gönderme işlemi
                var result = await _emailService.SendEmailAsync(mailModel);

                if (result.Success)
                {
                    return Json(new { success = true, message = "Hedef tarihi değişikliği talebiniz başarıyla gönderildi." });
                }
                else
                {
                    return Json(new { success = false, message = "Mail gönderilemedi. Lütfen tekrar deneyin." });
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yapabilir ve hata mesajı dönebilirsiniz
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

         

    }

}
