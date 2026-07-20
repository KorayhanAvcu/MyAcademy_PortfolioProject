using System.ComponentModel.DataAnnotations;

namespace Portfolio.Data.Entities
{
    public class Testimonial
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Puan alanı zorunludur.")]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Yorum alanı zorunludur.")]
        [StringLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Baş harfler alanı zorunludur.")]
        [StringLength(5, ErrorMessage = "Baş harfler en fazla 5 karakter olabilir.")]
        public string Initials { get; set; }

        [Required(ErrorMessage = "Ünvan alanı zorunludur.")]
        public string Title { get; set; }
    }
}