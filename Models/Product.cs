using System.ComponentModel.DataAnnotations;

namespace ButikStok.Models
{
    public class Product
    {
        public int Id { get; set; }

        // --- DÜZELTİLEN YER BURASI ---
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; } = ""; // <-- Sonuna = ""; ekledik, hata gitti!
        // -----------------------------

        [Display(Name = "Açıklama")]
        public string? Description { get; set; } // Soru işareti (?) boş olabilir demek

        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Display(Name = "Stok Adedi")]
        public int StockQuantity { get; set; }

        [Display(Name = "Resim")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Barkod No")]
        public string? Barcode { get; set; } 

        // İlişkiler
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }
        
        public virtual Category? Category { get; set; } // Bu da boş olabilir
    }
}