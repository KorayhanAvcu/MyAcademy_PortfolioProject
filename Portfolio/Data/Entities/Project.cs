using System.ComponentModel.DataAnnotations;

namespace Portfolio.Data.Entities
{
    public class Project
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Görsel Url boş bırakılamaz.")]
        public string ImageUrl{ get; set; }
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
