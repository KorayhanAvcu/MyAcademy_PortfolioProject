namespace Portfolio.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Yeniden calistirilan durum kodu sayfasindan gelen HTTP durum kodu (404, 403 ...).
        /// Islenmemis bir istisnada null.
        /// </summary>
        public int? StatusCode { get; set; }
    }
}
