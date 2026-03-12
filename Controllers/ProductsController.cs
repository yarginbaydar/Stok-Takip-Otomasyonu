using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ButikStok.Data;
using ButikStok.Models;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace ButikStok.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. LİSTELEME VE ARAMA ---
        public async Task<IActionResult> Index(string? aramaTerimi)
        {
            var urunler = _context.Products.Include(c => c.Category).AsQueryable();

            if (!string.IsNullOrEmpty(aramaTerimi))
            {
                // İsim, barkod veya kategoriye göre arama yapar
                urunler = urunler.Where(s => s.Name.Contains(aramaTerimi) || 
                                             (s.Barcode != null && s.Barcode.Contains(aramaTerimi)) ||
                                             (s.Category != null && s.Category.Name.Contains(aramaTerimi)));
            }
            
            // Satış ekranındaki açılır kutu için müşteri listesini gönderir
            ViewBag.Musteriler = new SelectList(_context.Customers.OrderBy(c => c.Name), "Id", "Name");

            ViewData["MevcutFiltre"] = aramaTerimi;
            return View(await urunler.ToListAsync());
        }

        // --- 2. ÜRÜN DETAYLARI VE QR KOD ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (product == null) return NotFound();

            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    string domain = Request.Host.Value ?? "localhost";
                    string protokol = Request.Scheme ?? "http";
                    string urunLinki = $"{protokol}://{domain}/Products/Details/{product.Id}";
                    
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(urunLinki, QRCodeGenerator.ECCLevel.Q);
                    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                    byte[] qrCodeImage = qrCode.GetGraphic(20);
                    
                    ViewBag.QrCode = Convert.ToBase64String(qrCodeImage);
                }
            }
            catch
            {
                ViewBag.QrCode = null;
            }

            return View(product);
        }

        // --- 3. YENİ ÜRÜN EKLEME ---
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,StockQuantity,ImageUrl,Barcode,CategoryId")] Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    product.ImageUrl = fileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = $"{product.Name} başarıyla eklendi.";
                TempData["Durum"] = "success";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // --- 4. DÜZENLEME ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,StockQuantity,ImageUrl,Barcode,CategoryId")] Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var oldProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        if (oldProduct != null && !string.IsNullOrEmpty(oldProduct.ImageUrl))
                        {
                            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", oldProduct.ImageUrl);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        product.ImageUrl = fileName;
                    }
                    else
                    {
                        var existingProd = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        if(existingProd != null) product.ImageUrl = existingProd.ImageUrl;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                TempData["Mesaj"] = "Ürün bilgileri güncellendi.";
                TempData["Durum"] = "info";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // --- 5. SİLME ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", product.ImageUrl);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Ürün silindi.";
                TempData["Durum"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 6. HIZLI SATIŞ (MÜŞTERİ VE BORÇ ENTEGRASYONLU) ---
        [HttpPost]
        public async Task<IActionResult> SatisYap(int id, int adet, int? CustomerId)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            if (product.StockQuantity >= adet)
            {
                product.StockQuantity -= adet;
                
                // Naming çakışmasını önlemek için tam namespace kullanıldı
                var islem = new ButikStok.Models.Transaction 
                {
                    ProductId = id,
                    Quantity = adet,
                    Type = "Satis",
                    Date = DateTime.Now,
                    CustomerId = CustomerId 
                };

                // Eğer müşteri seçildiyse borç bakiyesini güncelle
                if (CustomerId != null)
                {
                    var musterim = await _context.Customers.FindAsync(CustomerId);
                    if (musterim != null)
                    {
                        musterim.DebtBalance += (product.Price * adet);
                        _context.Update(musterim);
                    }
                }

                _context.Add(islem);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = $"{product.Name} satıldı! Kalan: {product.StockQuantity}";
                TempData["Durum"] = "success";
            }
            else
            {
                TempData["Mesaj"] = "HATA: Yetersiz Stok!";
                TempData["Durum"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 7. STOK EKLE ---
        [HttpPost]
        public async Task<IActionResult> StokEkle(int id, int adet)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.StockQuantity += adet;
            var islem = new ButikStok.Models.Transaction
            {
                ProductId = id,
                Quantity = adet,
                Type = "Giris",
                Date = DateTime.Now
            };
            _context.Add(islem);
            await _context.SaveChangesAsync();

            TempData["Mesaj"] = $"{adet} adet {product.Name} eklendi.";
            TempData["Durum"] = "info";

            return RedirectToAction(nameof(Index));
        }

        // --- 8. EXCEL DIŞA AKTAR (NPOI) ---
        public async Task<IActionResult> ExportToExcel()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            
            IWorkbook workbook = new XSSFWorkbook();
            ISheet excelSheet = workbook.CreateSheet("Stok Listesi");

            ICellStyle headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.RoyalBlue.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            IFont headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerFont.Color = IndexedColors.White.Index;
            headerStyle.SetFont(headerFont);

            IRow row = excelSheet.CreateRow(0);
            string[] headers = { "Barkod", "Ürün Adı", "Kategori", "Birim Fiyat", "Stok", "Değer" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                ICell cell = row.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            int rowIndex = 1;
            foreach (var item in products)
            {
                IRow dataRow = excelSheet.CreateRow(rowIndex);
                dataRow.CreateCell(0).SetCellValue(item.Barcode ?? "-");
                dataRow.CreateCell(1).SetCellValue(item.Name);
                dataRow.CreateCell(2).SetCellValue(item.Category?.Name ?? "Genel");
                dataRow.CreateCell(3).SetCellValue((double)item.Price);
                dataRow.CreateCell(4).SetCellValue(item.StockQuantity);
                dataRow.CreateCell(5).SetCellValue((double)(item.Price * item.StockQuantity));
                rowIndex++;
            }

            for(int i=0; i<headers.Length; i++) excelSheet.AutoSizeColumn(i);

            using (var stream = new MemoryStream())
            {
                workbook.Write(stream);
                return File(stream.ToArray(), 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"ButikStok_{DateTime.Now:ddMMyyyy}.xlsx");
            }
        }

        // --- 9. EXCEL İÇE AKTAR ---
        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Mesaj"] = "Lütfen bir Excel dosyası seçin!";
                TempData["Durum"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var defaultCategory = await _context.Categories.FirstOrDefaultAsync();
                int defaultCatId = defaultCategory != null ? defaultCategory.Id : 1; 

                using (var stream = excelFile.OpenReadStream())
                {
                    IWorkbook workbook = new XSSFWorkbook(stream);
                    ISheet sheet = workbook.GetSheetAt(0);

                    int eklenenSayisi = 0;
                    int guncellenenSayisi = 0;

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;

                        string barkod = row.GetCell(0)?.ToString() ?? "";
                        string ad = row.GetCell(1)?.ToString() ?? "İsimsiz Ürün";
                        decimal fiyat = 0;
                        int stok = 0;

                        if(row.GetCell(2) != null) decimal.TryParse(row.GetCell(2).ToString(), out fiyat);
                        if(row.GetCell(3) != null) int.TryParse(row.GetCell(3).ToString(), out stok);

                        if(string.IsNullOrWhiteSpace(barkod) && string.IsNullOrWhiteSpace(ad)) continue;

                        var mevcutUrun = await _context.Products.FirstOrDefaultAsync(p => p.Barcode == barkod && !string.IsNullOrEmpty(barkod));

                        if (mevcutUrun != null)
                        {
                            mevcutUrun.StockQuantity += stok;
                            if(fiyat > 0) mevcutUrun.Price = fiyat;
                            _context.Update(mevcutUrun);
                            guncellenenSayisi++;
                        }
                        else
                        {
                            var yeniUrun = new Product
                            {
                                Name = ad,
                                Barcode = barkod,
                                Price = fiyat,
                                StockQuantity = stok,
                                CategoryId = defaultCatId, 
                                Description = "Excel'den aktarıldı"
                            };
                            _context.Add(yeniUrun);
                            eklenenSayisi++;
                        }
                    }
                    await _context.SaveChangesAsync();
                    
                    TempData["Mesaj"] = $"İşlem Tamam! {eklenenSayisi} yeni ürün, {guncellenenSayisi} güncelleme.";
                    TempData["Durum"] = "success";
                }
            }
            catch (Exception ex)
            {
                TempData["Mesaj"] = "Hata: " + ex.Message;
                TempData["Durum"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // --- 10. ÜRÜN ETİKETİ VE YAZICI GÖRÜNÜMÜ ---
        public async Task<IActionResult> Label(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            string qrIcerik = product.Barcode ?? product.Id.ToString();

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrIcerik, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                ViewBag.QrCode = Convert.ToBase64String(qrCodeImage);
            }

            return View(product);
        }
    }
}