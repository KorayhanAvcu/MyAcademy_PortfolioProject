using System.ComponentModel.DataAnnotations;

namespace Portfolio.Models
{
    /// <summary>
    /// Public iletisim formunun modeli. Dogrulama nitelikleri
    /// <c>UserMessage</c> ile birebir ayni; fazladan yalnizca honeypot alani var.
    /// Form artik entity'ye dogrudan baglanmiyor: ziyaretci <c>IsRead</c> gibi
    /// alanlara deger gonderemez.
    /// </summary>
    public class ContactFormVM
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mesaj alanı zorunludur.")]
        [StringLength(100, MinimumLength = 10,
            ErrorMessage = "Mesaj en az 10, en fazla 100 karakter olabilir.")]
        [Display(Name = "Mesaj")]
        public string MessageBody { get; set; } = string.Empty;

        /// <summary>
        /// Honeypot: formda ekran disinda ve <c>aria-hidden</c>. Insan ziyaretci bu
        /// alani gormez, otomatik bot genellikle doldurur. Dolu gelirse mesaj
        /// kaydedilmez ama gonderen basarili sanir.
        /// </summary>
        public string? Website { get; set; }
    }
}
