using DESTAP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace DESTAP.Controllers
{
    [Authorize(Roles = "1")]  // Sadece admin rolündeki kullanıcılar erişebilir
    public class AdminController : Controller
    {
        private readonly DB_Context _context;

        public AdminController(DB_Context context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var records = _context.TB_ChangeTrack.ToList();  // Tüm kayıtları al
            return View(records);  // View'a gönder
        }
    }
}
