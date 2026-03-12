using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ButikStok.Data;
using ButikStok.Models;
using Microsoft.AspNetCore.Authorization;

namespace ButikStok.Controllers
{
    [Authorize] // Sadece giriş yapanlar görebilir!
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kullanıcıları Listele
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // Yeni Kullanıcı Ekleme Sayfası
        public IActionResult Create()
        {
            return View();
        }

        // Yeni Kullanıcıyı Kaydet
        [HttpPost]
        public async Task<IActionResult> Create(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var newUser = new User { Username = username, Password = password };
                _context.Add(newUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Hata = "Kullanıcı adı veya şifre boş olamaz!";
            return View();
        }

        // Kullanıcı Sil
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Kendimizi silmeyelim :)
                if (user.Username == User.Identity?.Name)
                {
                    TempData["Hata"] = "Kendini silemezsin!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}