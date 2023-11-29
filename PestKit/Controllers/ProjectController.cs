using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PestKit.Data;

namespace PestKit.Controllers
{
    public class ProjectController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Projects.Include(p=>p.ProjectImages).ToList());
        }
    }
}
