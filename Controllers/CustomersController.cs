using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ButikStok.Data;
using ButikStok.Models;

namespace ButikStok.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Müşteri Listesi (Arama Özellikli ve Tarihe Göre Sıralı)
        public async Task<IActionResult> Index(string? arama) 
        {
            var musteriler = from m in _context.Customers
                             select m;

            if (!string.IsNullOrEmpty(arama))
            {
                // Telefon null olsa bile hata vermeyen güvenli arama
                musteriler = musteriler.Where(s => 
                    s.Name.Contains(arama) || 
                    (s.PhoneNumber != null && s.PhoneNumber.Contains(arama))
                );
            }

            // En son eklenen müşteri en üstte gözüksün
            return View(await musteriler.OrderByDescending(x => x.RegisterDate).ToListAsync());
        }

        // 2. Yeni Müşteri Ekleme Sayfası (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. Yeni Müşteri Kaydetme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,PhoneNumber,Email,Notes")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.RegisterDate = DateTime.Now; // Kayıt tarihini otomatik ata
                _context.Add(customer);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Müşteri başarıyla kaydedildi!";
                TempData["Durum"] = "success";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // 4. Müşteri Detayı ve Geçmişi
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.Transactions) 
                .ThenInclude(t => t.Product)  
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // 5. Müşteri Silme Ekranı (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // 6. MÜŞTERİ SİLME ONAYI (POST) - HATANIN DÜZELTİLDİĞİ YER
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. Müşteriyi bul
            var customer = await _context.Customers.FindAsync(id);
            
            if (customer != null)
            {
                try 
                {
                    // 2. Önce bu müşteriye ait tüm alışveriş kayıtlarını bul ve sil
                    // (Hata almanı engelleyen kritik adım burası)
                    var islemler = _context.Transactions.Where(t => t.CustomerId == id);
                    _context.Transactions.RemoveRange(islemler);

                    // 3. Şimdi müşteriyi güvenle silebiliriz
                    _context.Customers.Remove(customer);
                    
                    await _context.SaveChangesAsync();
                    
                    TempData["Mesaj"] = "Müşteri ve tüm alışveriş geçmişi silindi.";
                    TempData["Durum"] = "danger";
                }
                catch (Exception)
                {
                    TempData["Mesaj"] = "Bir hata oluştu, müşteri silinemedi.";
                    TempData["Durum"] = "warning";
                }
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}