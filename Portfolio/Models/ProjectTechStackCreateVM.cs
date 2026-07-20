namespace Portfolio.Models
{
    public class ProjectTechStackCreateVM
    {
        public int ProjectId { get; set; }

        public List<int> TechStackIds { get; set; } = new();
    }
}
