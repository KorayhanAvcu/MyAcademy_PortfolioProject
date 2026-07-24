namespace Portfolio.Models
{
    public class DashboardViewModel
    {
        public int TotalProjects { get; set; }
        public int TotalExperiences { get; set; }
        public int TotalSkills { get; set; }
        public int TotalMessages { get; set; }
        public int UnreadMessages { get; set; }
        public int TotalServices { get; set; }
        public int TotalTestimonials { get; set; }

        public List<RecentProjectDto> RecentProjects { get; set; } = new();
        public List<RecentMessageDto> RecentMessages { get; set; } = new();
        public List<MonthlyMessageCountDto> MessagesByMonth { get; set; } = new();
    }

    public class RecentProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImgUrl { get; set; }
    }

    public class RecentMessageDto
    {
        public int Id { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
    }

    

    public class MonthlyMessageCountDto
    {
        public string MonthLabel { get; set; }   // "Oca", "Şub" vs.
        public int Count { get; set; }
    }
}
