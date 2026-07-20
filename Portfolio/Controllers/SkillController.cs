using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using Portfolio.Models;

namespace Portfolio.Controllers
{
    public class SkillController : Controller
    {
        private readonly AppDbContext _context;

        public SkillController(AppDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            var skills = _context.Skills
                .Include(x => x.Category)
                .Select(x => new SkillVM
                {
                    Id = x.Id,
                    SkillName = x.Name,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name
                })
                .ToList();

            return View(skills);
        }


        public IActionResult CreateSkill()
        {
            ViewBag.categories = _context.Categories
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
                .ToList();

            return View();
        }


        [HttpPost]
        public IActionResult CreateSkill(Skill skill)
        {
            _context.Skills.Add(skill);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }



        // UPDATE GET
        [HttpGet]
        public IActionResult UpdateSkill(int id)
        {
            var skill = _context.Skills
                .FirstOrDefault(x => x.Id == id);


            if (skill == null)
            {
                return NotFound();
            }


            ViewBag.categories = _context.Categories
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = x.Id == skill.CategoryId
                })
                .ToList();


            return View(skill);
        }



        // UPDATE POST
        [HttpPost]
        public IActionResult UpdateSkill(Skill skill)
        {
            var value = _context.Skills
                .FirstOrDefault(x => x.Id == skill.Id);


            if (value == null)
            {
                return NotFound();
            }


            value.Name = skill.Name;
            value.CategoryId = skill.CategoryId;


            _context.SaveChanges();


            return RedirectToAction("Index");
        }



        // DELETE
        public IActionResult DeleteSkill(int id)
        {
            var skill = _context.Skills
                .FirstOrDefault(x => x.Id == id);


            if (skill == null)
            {
                return NotFound();
            }


            _context.Skills.Remove(skill);
            _context.SaveChanges();


            return RedirectToAction("Index");
        }
    }
}