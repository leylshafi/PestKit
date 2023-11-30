using Microsoft.AspNetCore.Mvc;
using PestKit.Data;

namespace PestKit.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class EmployeeController : Controller
	{
		private readonly AppDbContext _context;

		public EmployeeController(AppDbContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			return View();
		}
	}
}
