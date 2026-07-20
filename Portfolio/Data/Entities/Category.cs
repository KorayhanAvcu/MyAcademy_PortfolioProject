using System.ComponentModel.DataAnnotations;

namespace Portfolio.Data.Entities
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        public string Name { get; set; }
        public List<Skill> Skills { get; set; }
    }
}
