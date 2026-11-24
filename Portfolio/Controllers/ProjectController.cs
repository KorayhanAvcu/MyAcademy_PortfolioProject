using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using Portfolio.Services;

namespace Portfolio.Controllers
{
    public class ProjectController : AdminControllerBase
    {
        private const string UploadFolder = "project";

        private readonly AppDbContext _context;
        private readonly IImageStorage _imageStorage;

        public ProjectController(AppDbContext context, IImageStorage imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        public IActionResult Index()
        {
            var projects = _context.Projects.ToList();
            return View(projects);
        }

        [HttpGet]
        public IActionResult CreateProject()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(Project project, CancellationToken ct)
        {
            // ImageUrl'in kendi zorunlulugu kalkti: dosya ya da URL yeterli.
            ModelState.Remove(nameof(Project.ImageUrl));

            if (!ModelState.IsValid)
            {
                return View(project);
            }

            if (project.ImageFile != null)
            {
                var result = await _imageStorage.SaveAsync(project.ImageFile, UploadFolder, ct);
                if (!result.Success)
                {
                    ModelState.AddModelError(nameof(Project.ImageFile), result.ErrorMessage!);
                    return View(project);
                }

                project.ImageUrl = result.RelativeUrl!;
            }

            if (string.IsNullOrWhiteSpace(project.ImageUrl))
            {
                ModelState.AddModelError(nameof(Project.ImageUrl), ImageRequiredMessage);
                return View(project);
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync(ct);
            return RedirectToAction("Index");
        }

        public IActionResult UpdateProject(int id)
        {
            var project = _context.Projects.Find(id);
            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProject(Project project, CancellationToken ct)
        {
            ModelState.Remove(nameof(Project.ImageUrl));

            if (!ModelState.IsValid)
            {
                return View(project);
            }

            var current = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == project.Id, ct);

            if (current == null)
            {
                return NotFound();
            }

            // Yeni dosya basariyla kaydedilirse eski yuklenmis dosya kayittan SONRA silinir.
            string? replacedUpload = null;

            if (project.ImageFile != null)
            {
                var result = await _imageStorage.SaveAsync(project.ImageFile, UploadFolder, ct);
                if (!result.Success)
                {
                    ModelState.AddModelError(nameof(Project.ImageFile), result.ErrorMessage!);
                    return View(project);
                }

                replacedUpload = current.ImageUrl;
                project.ImageUrl = result.RelativeUrl!;
            }
            else if (string.IsNullOrWhiteSpace(project.ImageUrl))
            {
                // Dosya secilmedi ve URL bos geldi: mevcut gorsel aynen korunur.
                project.ImageUrl = current.ImageUrl;
            }

            if (string.IsNullOrWhiteSpace(project.ImageUrl))
            {
                ModelState.AddModelError(nameof(Project.ImageUrl), ImageRequiredMessage);
                return View(project);
            }

            _context.Projects.Update(project);
            await _context.SaveChangesAsync(ct);

            _imageStorage.Delete(replacedUpload);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteProject(int id)
        {
            var project = _context.Projects.Find(id);
            if (project == null)
            {
                return NotFound();
            }

            var imageUrl = project.ImageUrl;

            _context.Projects.Remove(project);
            _context.SaveChanges();

            _imageStorage.Delete(imageUrl);
            return RedirectToAction("Index");
        }
    }
}
