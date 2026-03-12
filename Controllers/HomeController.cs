using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ButikStok.Models;
using ButikStok.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ButikStok.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // 1. Temel İstatistikler (Mevcut Kodların)
        var toplamUrun = _context.Products.Count();
        var kritikStok = _context.Products.Count(p => p.StockQuantity <= 5 && p.StockQuantity > 0);
        var toplamMaliyet = _context.Products.Sum(p => (decimal?)p.Price * p.StockQuantity) ?? 0;

        // --- YENİ EKLENEN: Müşteri Sayısı ---
        var musteriSayisi = await _context.Customers.CountAsync();

        // 2. Son Hareketler (Son 10 işlem)
        // --- GÜNCELLENEN: .Include(t => t.Customer) eklendi ---
        var sonIslemler = await _context.Transactions
            .Include(t => t.Product)
            .Include(t => t.Customer) // Müşteri ismini çekmek için bunu ekledik
            .OrderByDescending(t => t.Date)
            .Take(10)
            .ToListAsync();

        // 3. Grafik Verileri (Senin yazdığın kod aynen duruyor)
        var grafikVerisi = _context.Products
            .Include(p => p.Category)
            .Where(p => p.Category != null)
            .GroupBy(p => p.Category!.Name)
            .Select(g => new { KategoriAdi = g.Key, Sayi = g.Count() })
            .ToList();

        // 4. Verileri View'a taşı
        ViewBag.GrafikIsimler = grafikVerisi.Select(x => x.KategoriAdi).ToArray();
        ViewBag.GrafikSayilar = grafikVerisi.Select(x => x.Sayi).ToArray();
        
        ViewBag.ToplamUrun = toplamUrun;
        ViewBag.KritikStok = kritikStok;
        ViewBag.ToplamMaliyet = toplamMaliyet;
        
        // --- YENİ EKLENEN: View'a gönderiyoruz ---
        ViewBag.MusteriSayisi = musteriSayisi;

        return View(sonIslemler);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}