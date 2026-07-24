using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Context;
using Portfolio.Models;


namespace Portfolio.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-5);

            var vm = new DashboardViewModel
            {
                TotalProjects = await _context.Projects.CountAsync(),
                TotalExperiences = await _context.Experiences.CountAsync(),
                TotalSkills = await _context.Skills.CountAsync(),
                TotalServices = await _context.Services.CountAsync(),
                TotalTestimonials = await _context.Testimonials.CountAsync(),
                TotalMessages = await _context.UserMessages.CountAsync(),
                UnreadMessages = await _context.UserMessages.CountAsync(x => !x.IsRead),

                RecentProjects = await _context.Projects
                    .OrderByDescending(p => p.Id)
                    .Take(5)
                    .Select(p => new RecentProjectDto
                    {
                        Id = p.Id,
                        Title = p.Name,
                        Description = p.Description,
                        ImgUrl = p.ImageUrl
                    })
                    .ToListAsync(),

                RecentMessages = await _context.UserMessages
                    .OrderByDescending(m => m.CreatedDate)
                    .Take(5)
                    .Select(m => new RecentMessageDto
                    {
                        Id = m.Id,
                        SenderName = m.Name,
                        Subject = m.MessageBody,
                        CreatedDate = m.CreatedDate,
                        IsRead = m.IsRead
                    })
                    .ToListAsync(),

                
            };

            // Son 6 ay için mesaj istatistiği
            var monthlyRaw = await _context.UserMessages
                .Where(m => m.CreatedDate >= sixMonthsAgo)
                .GroupBy(m => new { m.CreatedDate.Year, m.CreatedDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var turkishMonths = new[] { "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };

            vm.MessagesByMonth = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-5 + i))
                .Select(d =>
                {
                    var found = monthlyRaw.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
                    return new MonthlyMessageCountDto
                    {
                        MonthLabel = turkishMonths[d.Month - 1],
                        Count = found?.Count ?? 0
                    };
                })
                .ToList();

            return View(vm);
        }
    }
}