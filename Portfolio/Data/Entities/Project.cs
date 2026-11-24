using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Data.Entities
{
    public class Project
    {
        public int Id { get; set; }
        // Dosya yukleme veya harici URL: ikisinden biri yeterli oldugu icin bu alan
        // artik tek basina zorunlu degil, kontrol controller'da iki alana birlikte bakar.
        public string ImageUrl{ get; set; }

        /// <summary>Panelden yuklenen gorsel. Veritabanina yazilmaz.</summary>
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        [Required(ErrorMessage = "Proje Adı boş bırakılamaz.")]
        [MinLength(3,ErrorMessage ="Proje Adı en az 3 karakter  olmalıdır.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Proje Açıklaması boş bırakılamaz.")]
        [MaxLength(100, ErrorMessage ="Proje Açıklaması en fazla 100 karakter olmalıdır.")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Github Url boş bırakılamaz.")]
        public string GithubUrl { get; set; }
        public List<ProjectTechStack>? ProjectTechStacks { get; set; } 
    }
}
