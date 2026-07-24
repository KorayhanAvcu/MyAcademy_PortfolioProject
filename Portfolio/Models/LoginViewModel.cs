using System.ComponentModel.DataAnnotations;

namespace Portfolio.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Kullanıcı adı boş geçilemez.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Şifre boş geçilemez.")]
        public string Password { get; set; }
    }
}
