using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using Portfolio.Models;

namespace Portfolio.Controllers
{
    public class ProjectTechStacksController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectTechStacksController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var projectTechStacks = _context.ProjectTechStacks.Include(x => x.Project)
                .Include(y => y.TechStack)
                .GroupBy(z => new { z.ProjectId, z.Project.Name })
                .Select(a => new ProjectTechStackVM
                {
                    ProjectId = a.Key.ProjectId,
                    ProjectName = a.Key.Name,
                    TechStacks = string.Join(",", a.Select(x=>x.TechStack.Name))
                }).ToList();
            return View(projectTechStacks);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var projects = _context.Projects.ToList();
            var techStacks = _context.TechStacks.ToList();

            ViewBag.projects = (from project in projects
                                select new SelectListItem
                                {
                                    Text = project.Name,
                                    Value = project.Id.ToString()
                                }).ToList();

            ViewBag.techStacks = (from tech in techStacks
                                  select new SelectListItem
                                  {
                                      Text = tech.Name,
                                      Value = tech.Id.ToString()
                                  }).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProjectTechStackCreateVM model)
        {
            foreach (var techStackId in model.TechStackIds)
            {
                _context.ProjectTechStacks.Add(new ProjectTechStack
                {
                    ProjectId = model.ProjectId,
                    TechStackId = techStackId
                });
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            var model = new ProjectTechStackCreateVM();

            model.ProjectId = id;

            model.TechStackIds = _context.ProjectTechStacks
                .Where(x => x.ProjectId == id)
                .Select(x => x.TechStackId)
                .ToList();

            ViewBag.projects = _context.Projects.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            ViewBag.techStacks = _context.TechStacks.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            return View(model);
        }
        [HttpPost]
        public IActionResult Update(ProjectTechStackCreateVM model)
        {
            var oldValues = _context.ProjectTechStacks
                .Where(x => x.ProjectId == model.ProjectId)
                .ToList();

            _context.ProjectTechStacks.RemoveRange(oldValues);

            foreach (var techId in model.TechStackIds)
            {
                _context.ProjectTechStacks.Add(new ProjectTechStack
                {
                    ProjectId = model.ProjectId,
                    TechStackId = techId
                });
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var values = _context.ProjectTechStacks
                .Where(x => x.ProjectId == id)
                .ToList();

            _context.ProjectTechStacks.RemoveRange(values);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
