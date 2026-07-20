using System.ComponentModel.DataAnnotations;

namespace Portfolio.Data.Entities
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "İkon alanı zorunludur.")]
        public string Icon { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Açıklama alanı zorunludur.")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string Description { get; set; }
    }
}