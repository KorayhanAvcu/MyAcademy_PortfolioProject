using System.ComponentModel.DataAnnotations;

namespace Portfolio.Data.Entities
{
    public class ContactInfo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Adres bilgisi zorunludur.")]
        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "LinkedIn adresi zorunludur.")]
        [Url(ErrorMessage = "Geçerli bir LinkedIn URL'si giriniz.")]
        [StringLength(200, ErrorMessage = "LinkedIn URL'si en fazla 200 karakter olabilir.")]
        public string LinkedinUrl { get; set; }

        [Required(ErrorMessage = "GitHub adresi zorunludur.")]
        [Url(ErrorMessage = "Geçerli bir GitHub URL'si giriniz.")]
        [StringLength(200, ErrorMessage = "GitHub URL'si en fazla 200 karakter olabilir.")]
        public string GithubUrl { get; set; }
    }
}