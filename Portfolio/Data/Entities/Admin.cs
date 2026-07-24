namespace Portfolio.Data.Entities
{
    public class Admin
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string? ImageUrl { get; set; }
    }
}
