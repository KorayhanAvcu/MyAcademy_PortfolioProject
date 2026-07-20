using System.ComponentModel.DataAnnotations;

namespace Portfolio.Data.Entities
{
    public class Education
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Okul alanı boş bırakılamaz.")]
        public string SchoolName { get; set; }
        [Required(ErrorMessage = "Bölüm alanı boş bırakılamaz.")]
        public string Department { get; set; }
        [Required(ErrorMessage = "GPA alanı boş bırakılamaz.")]
        public double GPA { get; set; }
        [Required(ErrorMessage = "Başlangıç yılı alanı boş bırakılamaz.")]
        public int StartYear { get; set; }
        public string? GraduationYear { get; set; }
        [Required(ErrorMessage = "Eğitim alanı Açıklaması boş bırakılamaz.")]
        [MaxLength(100, ErrorMessage = "Eğitim alanı Açıklaması en fazla 100 karakter olmalıdır.")]
        public string Description { get; set; }
    }
}
