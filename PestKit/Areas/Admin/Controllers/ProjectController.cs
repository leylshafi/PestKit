using Microsoft.AspNetCore.Authorization;
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
		[Authorize(Roles ="Admin,Moderator")]
		public async Task<IActionResult> Index()
		{
			var projects = await _context.Projects.Include(p=>p.ProjectImages).ToListAsync();
			return View(projects);
		}
		[Authorize(Roles ="Admin,Moderator")]
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
		[Authorize(Roles ="Admin,Moderator")]
		public async Task<IActionResult>Details(int id)
		{
			if (id <= 0) return BadRequest();
			var existed = await _context.Projects.Include(p=>p.ProjectImages).FirstOrDefaultAsync(p => p.Id == id);
			if(existed == null) return NotFound();
			return View(existed);
		}
		[Authorize(Roles ="Admin,Moderator")]
		public async Task<IActionResult> Update(int id)
		{
			if(id<= 0) return BadRequest();	
			var existed = await _context.Projects.Include(p=>p.ProjectImages).FirstOrDefaultAsync(p => p.Id == id);
			if(existed == null) return NotFound();
			var vm = new UpdateProjectVM
			{
				Name = existed.Name,
				ProjectImages = existed.ProjectImages,
			};

			return View(vm);
		}
		[HttpPost]
		public async Task<IActionResult> Update(int id, UpdateProjectVM projectVM)
		{
			Project existed = await _context.Projects.Include(p=>p.ProjectImages).FirstOrDefaultAsync(p=>p.Id == id);
			projectVM.ProjectImages = existed.ProjectImages;
			if (!ModelState.IsValid) return View(projectVM);
			if (existed == null) return NotFound();
			bool result = _context.Projects.Any(c => c.Name == projectVM.Name && c.Id != id);
			if (result)
			{
				ModelState.AddModelError("Name", "There is already such project");
				return View(projectVM);
			}
			if (projectVM.MainPhoto is not null)
			{
				if (!projectVM.MainPhoto.ValidateType())
				{
					ModelState.AddModelError("MainPhoto", "File type is not valid");
					return View(projectVM);
				}
				if (!projectVM.MainPhoto.ValidateSize(600))
				{
					ModelState.AddModelError("MainPhoto", "File size is not valid");
					return View(projectVM);
				}
			}
			if (projectVM.MainPhoto is not null)
			{
				string fileName = await projectVM.MainPhoto.CreateFile(_env.WebRootPath, "assets", "img");
				ProjectImage mainImage = existed.ProjectImages.FirstOrDefault(pi => pi.IsPrimary == true);
				mainImage.Url.DeleteFile(_env.WebRootPath, "assets", "img");
				_context.ProjectImages.Remove(mainImage);
				existed.ProjectImages.Add(new ProjectImage
				{
					Alternative = projectVM.Name,
					IsPrimary = true,
					Url = fileName
				});
			}
			if (projectVM.ImageIds is null)
			{
				projectVM.ImageIds = new();
			}
			var removeable = existed.ProjectImages.Where(pi => !projectVM.ImageIds.Exists(imgId => imgId == pi.Id) && pi.IsPrimary == false).ToList();
			foreach (ProjectImage pi in removeable)
			{
				pi.Url.DeleteFile(_env.WebRootPath, "assets", "img");
				existed.ProjectImages.Remove(pi);
			}
			TempData["Message"] = "";
			if (projectVM.Photos is not null)
			{
				foreach (IFormFile photo in projectVM.Photos)
				{
					if (!photo.ValidateType())
					{
						TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file type wrong</p>";
						continue;
					}
					if (!photo.ValidateSize(600))
					{
						TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file size wrong</p>";
						continue;
					}

					existed.ProjectImages.Add(new ProjectImage
					{
						Alternative = projectVM.Name,
						IsPrimary = false,
						Url = await photo.CreateFile(_env.WebRootPath, "assets", "img")
					});
				}
			}

			existed.Name = projectVM.Name;
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		[Authorize(Roles ="Admin")]
		public async Task<IActionResult> Delete(int id)
		{
			if (id <= 0) return BadRequest();
			var existed = await _context.Projects.Include(p => p.ProjectImages).FirstOrDefaultAsync(p => p.Id == id);
			if (existed == null) return NotFound();
			foreach (ProjectImage image in existed.ProjectImages)
			{
				image.Url.DeleteFile(_env.WebRootPath, "assets", "img");
			}
			_context.Projects.Remove(existed);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
    }
}
