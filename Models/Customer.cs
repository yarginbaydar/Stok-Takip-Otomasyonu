using System.ComponentModel.DataAnnotations;

namespace ButikStok.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [Display(Name = "Müşteri Adı / Unvanı")]
        // DÜZELTME: = string.Empty; ekleyerek başlangıçta boş olacağını belirttik.
        public string Name { get; set; } = string.Empty; 

        [Display(Name = "Telefon No")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "E-Posta Adresi")]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Notlar")]
        public string? Notes { get; set; }

        [Display(Name = "Güncel Bakiye (Borç)")]
        public decimal DebtBalance { get; set; } = 0;

        [Display(Name = "Kayıt Tarihi")]
        public DateTime RegisterDate { get; set; } = DateTime.Now;

        // İlişki hatası almamak için listeyi de boş olarak başlatıyoruz
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}