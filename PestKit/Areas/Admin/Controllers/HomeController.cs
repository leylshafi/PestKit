using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PestKit.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class HomeController : Controller
	{
		[Authorize(Roles = "Admin,Moderator")]
		public IActionResult Index()
		{
			return View();
		}
	}
}
