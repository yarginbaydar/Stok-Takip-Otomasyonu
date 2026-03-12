using System.ComponentModel.DataAnnotations;

namespace ButikStok.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Kategori Adı")] // Sitede "Name" yerine bu yazacak
        [Required(ErrorMessage = "Lütfen bir kategori adı giriniz!")] // Boş bırakırsa uyaracak
        public string Name { get; set; } = ""; // = ""; diyerek o sarı hatayı çözdük
    }
}