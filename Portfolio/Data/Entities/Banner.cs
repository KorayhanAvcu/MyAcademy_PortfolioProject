using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Data.Entities
{
    public class Banner
    {
        public int Id { get; set; }

        // Dosya yukleme veya harici URL: ikisinden biri yeterli oldugu icin bu alan
        // artik tek basina zorunlu degil, kontrol controller'da iki alana birlikte bakar.
        public string ImageUrl { get; set; }

        /// <summary>Panelden yuklenen gorsel. Veritabanina yazilmaz.</summary>
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Açıklama alanı zorunludur.")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string Description { get; set; }
    }
}