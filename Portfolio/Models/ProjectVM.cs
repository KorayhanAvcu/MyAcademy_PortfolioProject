namespace Portfolio.Models
{
    public class ProjectVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        /// <summary>Kart uzerindeki "Projeyi Gor" baglantisinin hedefi.</summary>
        public string GithubUrl { get; set; }

        public List<string> TechStacks { get; set; } = new();
    }
}
