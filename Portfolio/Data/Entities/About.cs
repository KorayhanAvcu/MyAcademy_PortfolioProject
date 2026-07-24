using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Görsel Url boş bırakılamaz.")]
        public string ImageUrl { get; set; }

    }
}
