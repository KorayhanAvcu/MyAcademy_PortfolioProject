using System.ComponentModel.DataAnnotations;

namespace Portfolio.Data.Entities
{
    public class UserMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")] 
        public string Email { get; set; }

        [Required(ErrorMessage = "Mesaj alanı zorunludur.")]
        [StringLength(100, MinimumLength = 10,
            ErrorMessage = "Mesaj en az 10, en fazla 100 karakter olabilir.")]
        public string MessageBody { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}