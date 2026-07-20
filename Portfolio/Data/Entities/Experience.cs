using System.ComponentModel.DataAnnotations;

namespace Portfolio.Data.Entities
{
    public class Experience
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Başlık alanı boş bırakılamaz.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Deneyim Açıklaması boş bırakılamaz.")]
        [MaxLength(100, ErrorMessage = "Deneyim Açıklaması en fazla 100 karakter olmalıdır.")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Şirket alanı boş bırakılamaz.")]
        public string Company { get; set; }
        [Required(ErrorMessage = "Başlangıç yılı alanı boş bırakılamaz.")]
        public int StartYear { get; set; }
        public string? EndYear { get; set; }

    }
}
