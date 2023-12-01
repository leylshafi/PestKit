using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PestKit.Data;

namespace PestKit.Controllers
{
	public class DepartmentController : Controller
	{
		private readonly AppDbContext _context;

		public DepartmentController(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var departments = await _context.Departments.ToListAsync();
			return View(departments);
		}
	}
}
