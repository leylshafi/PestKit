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


    }
}
