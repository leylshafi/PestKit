using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PestKit.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ErrorPage(string error)
        {
            return View(model: error);
        }

    }
}
