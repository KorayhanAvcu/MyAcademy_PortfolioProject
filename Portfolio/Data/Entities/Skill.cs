using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Data.Entities
{
    public class Skill
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Skil alanı boş bırakılamaz.")]
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }

    }
}
