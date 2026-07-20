namespace Portfolio.Data.Entities
{
    public class ProjectTechStack
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public int TechStackId { get; set; }
        public TechStack TechStack { get; set; }

    }
}
