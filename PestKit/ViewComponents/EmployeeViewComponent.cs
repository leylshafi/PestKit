using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PestKit.Data;

namespace PestKit.ViewComponents
{
    public class EmployeeViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public EmployeeViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var employees = await _context.Employees.Include(e=>e.Position).ToListAsync();
            
            return View(employees);
        }
    }
}
