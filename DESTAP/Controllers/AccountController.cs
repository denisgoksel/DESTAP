using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using DESTAP.Models; // Veritabanı modeline erişim
using Microsoft.EntityFrameworkCore;

namespace DESTAP.Controllers
{
    public class AccountController : Controller
    {
        private readonly DB_Context _context;

        public AccountController(DB_Context context)
        {
            _context = context;
        }

        // Login GET
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Login POST
        [HttpPost]
        public async Task<IActionResult> Login(UserModel f_user)
        {
            // Kullanıcıyı veritabanında kontrol etme
            var user = _context.TB_Users.FirstOrDefault(u => u.Password == f_user.Password);

            if (user != null)
            {
                // Kullanıcı bulundu, giriş işlemi yapılacak
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.NameSurname),
                    new Claim(ClaimTypes.Email, user.Mail),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),  // Kullanıcı rolünü claim olarak ekliyoruz
                    new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()) ///Kullanıcı IDsi ni claime ekliyoruz.
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Authentication işlemi yapılır
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

           
                // Kullanıcı rolüne göre yönlendirme
                if (user.Role == 1)  // Admin rolü
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (user.Role == 2)  // Kullanıcı rolü
                {
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            // Kullanıcı doğrulanamazsa hata mesajı
            ModelState.AddModelError("Password", "Geçersiz Parola !");
            return View();
        }

        // Logout
        
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Kullanıcı çıkışı işlemi yapılır
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Access Denied (Yetkisiz erişim)
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Info(int id)
        {
            // Veritabanından kullanıcı bilgilerini getir
            var user = _context.TB_Users
                              .Include(u => u.RoleDetails) // RoleDetails ilişkisinin dahil edilmesi
                              .FirstOrDefault(u => u.ID == id);

            if (user == null)
            {
                return NotFound(); // Kullanıcı bulunamazsa 404 döndür
            }

            // Kullanıcı modelini "Info" görünümüne gönder
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string oldPassword, string newPassword)
        {
            var user = await _context.TB_Users.FindAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            // Eski şifre kontrolü
            if (user.Password != oldPassword)
            {
                return Json(new { success = false, message = "Eski şifre yanlış." });
            }
            var existingUser = _context.TB_Users.FirstOrDefault(u => u.ID == id);
            if (existingUser != null)
            {
                // Şifrenin benzersiz olup olmadığını kontrol et
                var passwordExists = _context.TB_Users.FirstOrDefault(u => u.Password == newPassword);
                if (passwordExists != null)
                { 
                    if (passwordExists.ID.ToString() != User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString())
                    {
                        //ModelState.AddModelError("Password", "Bu şifre zaten başka bir kullanıcıda mevcut. Lütfen farklı bir şifre girin.");
                        return Json(new { success = false, message = "Bu şifre zaten başka bir kullanıcıda mevcut. Lütfen farklı bir şifre girin." });
                    }
                }
            }

                // Şifre güncellemesi
            
            if (user.Password == newPassword) return Json(new { success = false, message = "İki şifrede aynı; dalga mı geçiyorsun!" });
            user.Password = newPassword;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Şifre başarıyla güncellendi." });
        }
    }
}
