using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PestKit.Areas.Admin.ViewModels;
using PestKit.Data;
using PestKit.Models;
using PestKit.Utilities.Extetions;

namespace PestKit.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProjectController : Controller
	{
		private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProjectController(AppDbContext context,IWebHostEnvironment env)
		{
			_context = context;
            _env = env;
        }

		public async Task<IActionResult> Index()
		{
			var projects = await _context.Projects.Include(p=>p.ProjectImages).ToListAsync();
			return View(projects);
		}

		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
        public async Task<IActionResult> Create(CreateProjectVM projectVM)
        {
			if(!ModelState.IsValid) return View();
			if(!projectVM.MainPhoto.ValidateType())
			{
				ModelState.AddModelError("MainPhoto", "Wrong file type");
				return View();
			}
            if (!projectVM.MainPhoto.ValidateSize(600))
            {
                ModelState.AddModelError("MainPhoto", "Wrong file size");
                return View();
            }

			ProjectImage main = new ProjectImage
			{
				Alternative = projectVM.Name,
				IsPrimary = true,
				Url = await projectVM.MainPhoto.CreateFile(_env.WebRootPath, "assets", "img"),
			};

			Project project = new Project
			{
				Name = projectVM.Name,
				ProjectImages = new()
				{
					main
				}
			};

            TempData["Message"] = "";
            foreach (IFormFile photo in projectVM.Photos)
            {
                if (!photo.ValidateType())
                {
                    TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file type wrong</p>";
                    continue;
                }
                if (!photo.ValidateSize(600))
                {
                    TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file type size</p>";
                    continue;
                }

				project.ProjectImages.Add(new ProjectImage
				{
					Alternative = projectVM.Name,
					IsPrimary = false,
					Url = await photo.CreateFile(_env.WebRootPath, "assets", "img"),
				});
            }

			_context.Projects.Add(project);
			await _context.SaveChangesAsync();
           return RedirectToAction(nameof(Index));
        }

		public async Task<IActionResult>Details(int id)
		{
			if (id <= 0) return BadRequest();
			var existed = await _context.Projects.Include(p=>p.ProjectImages).FirstOrDefaultAsync(p => p.Id == id);
			if(existed == null) return NotFound();
			return View(existed);
		}
    }
}
