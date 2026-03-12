using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ButikStok.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        // Hangi ürün? (ID'si)
        public int ProductId { get; set; }

        // İşte eksik olan parça buydu! (Ürünün kendisi)
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public int Quantity { get; set; } // Adet
        public DateTime Date { get; set; } = DateTime.Now;
        public string Type { get; set; } = "Satis"; // "Satis" veya "Giris"

        [Display(Name = "Müşteri")]
public int? CustomerId { get; set; } // Boş olabilir (Anonim satış)

[Display(Name = "Müşteri Bilgisi")]
public Customer? Customer { get; set; }

    }
}