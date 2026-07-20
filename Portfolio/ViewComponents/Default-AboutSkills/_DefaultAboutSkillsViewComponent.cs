using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Context;
using Portfolio.Models;

namespace Portfolio.ViewComponents.Default_AboutSkills
{
    public class _DefaultAboutSkillsViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public _DefaultAboutSkillsViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var skills = _context.Skills
                .Include(x => x.Category)
                .GroupBy(x => new
                {
                    x.CategoryId,
                    x.Category.Name
                })
                .Select(x => new SkillGroupVM
                {
                    CategoryId = x.Key.CategoryId,
                    CategoryName = x.Key.Name,
                    Skills = string.Join(", ", x.Select(y => y.Name))
                })
                .ToList();
            return View(skills);
        }
    }
}
