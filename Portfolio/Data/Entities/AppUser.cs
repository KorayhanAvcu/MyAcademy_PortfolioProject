using Microsoft.AspNetCore.Identity;

namespace Portfolio.Data.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
