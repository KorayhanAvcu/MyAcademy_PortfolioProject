using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;
using Portfolio.Models;

namespace Portfolio.ViewComponents.Default_Portfolio
{
    public class _DefaultPortfolioViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public _DefaultPortfolioViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var projects = _context.Projects
            .Select(p => new ProjectVM
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                TechStacks = _context.ProjectTechStacks
                    .Where(x => x.ProjectId == p.Id)
                    .Select(x => x.TechStack.Name)
                    .ToList()
            })
            .ToList();

            return View(projects);
        }
    }
}
