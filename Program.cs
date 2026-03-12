using ButikStok.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ADIM: YOL AYARI VE KLASÖR GARANTİSİ ---
var contentRoot = builder.Environment.ContentRootPath;
var appDataPath = Path.Combine(contentRoot, "App_Data");

// App_Data yoksa oluştur
if (!Directory.Exists(appDataPath))
{
    Directory.CreateDirectory(appDataPath);
}

// Veritabanı yolu
var dbPath = Path.Combine(appDataPath, "ButikStok.db");
var connectionString = $"Data Source={dbPath}";

// Veritabanı servisini ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// --- GÜVENLİK SERVİSİ ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- 🔥 KRİTİK DÜZELTME: VERİTABANI TABLOLARINI OLUŞTURMA 🔥 ---
// Bu blok site açılırken çalışır, tablolar eksikse hemen oluşturur.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Veritabanı yoksa oluşturur, tablolar yoksa yaratır!
        context.Database.EnsureCreated(); 
    }
    catch (Exception ex)
    {
        // Hata olursa loga yazar ama siteyi çökertmez
        Console.WriteLine("Veritabanı oluşturulurken hata: " + ex.Message);
    }
}
// -------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();