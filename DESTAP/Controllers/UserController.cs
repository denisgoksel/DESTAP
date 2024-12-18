using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DESTAP.Controllers
{
    [Authorize(Roles = "1,2")]  // Sadece kullanıcı rolündeki kullanıcılar erişebilir
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
