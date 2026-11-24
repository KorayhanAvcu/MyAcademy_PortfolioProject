namespace Portfolio.Models
{
    /// <summary>
    /// Sayfa basligi, aciklamasi ve Open Graph alanlari. Degerler
    /// <c>Site:*</c> yapilandirmasindan gelir; mutlak adresler <c>Site:BaseUrl</c>
    /// uzerinden kurulur. BaseUrl bos ise mutlak adres uretilmez ve ilgili
    /// etiketler hic yazilmaz (yanlis adres vermektense hic vermemek yegdir).
    /// </summary>
    public class PageMetaVM
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>Mutlak sayfa adresi (canonical ve og:url). BaseUrl yoksa null.</summary>
        public string? CanonicalUrl { get; set; }

        /// <summary>Mutlak paylasim gorseli adresi (og:image). Ayarli degilse null.</summary>
        public string? ImageUrl { get; set; }
    }
}
