using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ButikStok.Data;
using Microsoft.AspNetCore.Authorization; // KİLİT KÜTÜPHANESİ

namespace ButikStok.Controllers
{
    [Authorize] // <--- İŞTE KİLİT BURADA
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var islemler = await _context.Transactions
                .Include(t => t.Product)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return View(islemler);
        }
    }
}