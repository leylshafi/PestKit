using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PestKit.Areas.Admin.ViewModels;
using PestKit.Data;
using PestKit.Models;
using PestKit.Utilities.Extetions;
using System.Reflection.Metadata;

namespace PestKit.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class DepartmentController : Controller
	{
		private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _env;

		public DepartmentController(AppDbContext context, IWebHostEnvironment env)
		{
			_context = context;
			_env = env;
		}
		[Authorize(Roles = "Admin,Moderator")]
		public async Task<IActionResult> Index(int page)
		{
			double count = await _context.Departments.CountAsync();
			var departments = await _context.Departments.Skip(page*2).Take(2)
				.Include(d => d.Employees).ToListAsync();
			PaginationVM<Department> pagination = new PaginationVM<Department>()
			{
				CurrentPage = page,
				TotalPage = Math.Ceiling(count / 2),
				Items = departments
			};
			return View(pagination);
		}
		[Authorize(Roles = "Admin,Moderator")]
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(CreateDepartmentVM departmentVM)
		{
			if (!ModelState.IsValid) return View();
			if (!departmentVM.Photo.ValidateType())
			{
				ModelState.AddModelError("Photo", "Wrong file type");
				return View();
			}
			if (!departmentVM.Photo.ValidateSize(2 * 1024))
			{
				ModelState.AddModelError("Photo", "It shouldn't exceed 2 mb");
				return View();
			}
            bool result = _context.Departments.Any(c => c.Name == departmentVM.Name);
            if (result)
            {
                ModelState.AddModelError("Name", "There is already such department");
                return View();
            }
            var department = new Department
			{
				Name = departmentVM.Name,
				ImageUrl = await departmentVM.Photo.CreateFile(_env.WebRootPath, "assets", "img"),
			};
			await _context.Departments.AddAsync(department);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		[Authorize(Roles = "Admin,Moderator")]
		public async Task<IActionResult> Update(int id)
		{
			if (id <= 0) return BadRequest();
			var existed = await _context.Departments.FirstOrDefaultAsync(x => x.Id == id);
			if (existed == null) return NotFound();
			var vm = new UpdateDepartmentVM
			{
				Name = existed.Name,
				ImageUrl = existed.ImageUrl,
			};
			return View(vm);
		}
		[HttpPost]
		public async Task<IActionResult> Update(int id, UpdateDepartmentVM departmentVM)
		{
			if (!ModelState.IsValid) return View(departmentVM);

			var department = await _context.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.Id == id);
			if (department == null) return NotFound();

			bool result = _context.Departments.Any(c => c.Name == departmentVM.Name && c.Id != id);
			if (result)
			{
				ModelState.AddModelError("Name", "There is already such department");
				return View(departmentVM);
			}

			if (departmentVM.Photo != null)
			{
				if (!departmentVM.Photo.ValidateType())
				{
					ModelState.AddModelError("Photo", "Wrong file type");
					return View();
				}
				if (!departmentVM.Photo.ValidateSize(2 * 1024))
				{
					ModelState.AddModelError("Photo", "It shouldn't exceed 2 mb");
					return View();
				}

				department.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "img");
				department.ImageUrl = await departmentVM.Photo.CreateFile(_env.WebRootPath, "assets", "img");
			}
			department.Name = departmentVM.Name;
			

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int id)
		{
			if(id<=0) return BadRequest();
			var existed = await _context.Departments.FirstOrDefaultAsync(d=>d.Id== id);
			if (existed is null) return NotFound();
			_context.Departments.Remove(existed);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		[Authorize(Roles = "Admin,Moderator")]
		public async Task<IActionResult> Details(int id)
		{
			if (id <= 0) return BadRequest();
			var existed = await _context.Departments.Include(d=>d.Employees).FirstOrDefaultAsync(d => d.Id == id);
			if (existed is null) return NotFound();
			return View(existed);

		}
	}
}
