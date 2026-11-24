using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Data.Entities
{
    public class About
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Başık alanı boş bırakılamaz.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Hakkımda Açıklaması boş bırakılamaz.")]
        [MaxLength(2000, ErrorMessage = "Hakkımda Açıklaması en fazla 2000 karakter olmalıdır.")]
        public string Descripton { get; set; }
        // Dosya yukleme veya harici URL: ikisinden biri yeterli oldugu icin bu alan
        // artik tek basina zorunlu degil, kontrol controller'da iki alana birlikte bakar.
        public string ImageUrl { get; set; }

        /// <summary>Panelden yuklenen gorsel. Veritabanina yazilmaz.</summary>
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
