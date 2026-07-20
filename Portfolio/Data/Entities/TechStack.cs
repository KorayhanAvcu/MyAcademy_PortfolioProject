using System.ComponentModel.DataAnnotations;

namespace Portfolio.Data.Entities
{
    public class TechStack
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Teknoloji alanı boş bırakılamaz.")]
        public string Name { get; set; }
        public List<ProjectTechStack> ProjectTechStacks { get; set; }

    }
}
