using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ButikStok.Data; // Veritabanı erişimi
using ButikStok.Models; // User modeli erişimi

namespace ButikStok.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string kadi, string sifre)
        {
            // --- GÜVENLİK SİGORTASI ---
            // Eğer veritabanında hiç kullanıcı yoksa, otomatik 'admin' oluşturalım.
            // Böylece siteyi ilk kurduğunda kapıda kalmazsın.
            if (!_context.Users.Any())
            {
                var admin = new User { Username = "admin", Password = "1234" };
                _context.Users.Add(admin);
                _context.SaveChanges();
            }
            // ---------------------------

            // Veritabanında bu kullanıcı adı ve şifreye sahip biri var mı?
            var user = _context.Users.FirstOrDefault(u => u.Username == kadi && u.Password == sifre);

            if (user != null)
            {
                // Giriş Başarılı!
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username)
                };
                var userIdentity = new ClaimsIdentity(claims, "Login");
                ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

                await HttpContext.SignInAsync(principal);
                return RedirectToAction("Index", "Home");
            }

            // Giriş Başarısız
            ViewBag.Hata = "Kullanıcı adı veya şifre yanlış!";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}