using System.ComponentModel.DataAnnotations;

namespace Portfolio.Data.Entities
{
    public class Banner
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Görsel URL'si zorunludur.")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Açıklama alanı zorunludur.")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string Description { get; set; }
    }
}