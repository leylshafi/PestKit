using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PestKit.Areas.Admin.ViewModels;
using PestKit.Data;
using PestKit.Models;
using PestKit.Utilities.Extetions;
using System.Runtime.CompilerServices;

namespace PestKit.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public EmployeeController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .ToListAsync();
            return View(employees);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new CreateEmployeeVM
            {
                Positions = await _context.Positions.ToListAsync(),
                Departments = await _context.Departments.ToListAsync()
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeVM employeeVM)
        {
            if (!ModelState.IsValid)
            {
                employeeVM.Positions = await _context.Positions.ToListAsync();
                employeeVM.Departments = await _context.Departments.ToListAsync();
                return View(employeeVM);
            }
            bool result = await _context.Departments.AnyAsync(c => c.Id == employeeVM.DepartmentId);
            if (!result)
            {
                employeeVM.Positions = await _context.Positions.ToListAsync();
                employeeVM.Departments = await _context.Departments.ToListAsync();
                ModelState.AddModelError("DepartmentId", "There is no such deparment");
                return View(employeeVM);
            }

            result = await _context.Positions.AnyAsync(c => c.Id == employeeVM.PositionId);
            if (!result)
            {
                employeeVM.Positions = await _context.Positions.ToListAsync();
                employeeVM.Departments = await _context.Departments.ToListAsync();
                ModelState.AddModelError("PositionId", "There is no such position");
                return View(employeeVM);
            }

            if (!employeeVM.Image.ValidateType())
            {
                employeeVM.Positions = await _context.Positions.ToListAsync();
                employeeVM.Departments = await _context.Departments.ToListAsync();
                ModelState.AddModelError("Image", "Wrong file type");
                return View(employeeVM);
            }
            if (!employeeVM.Image.ValidateSize(600))
            {
                employeeVM.Positions = await _context.Positions.ToListAsync();
                employeeVM.Departments = await _context.Departments.ToListAsync();
                ModelState.AddModelError("Image", "Wrong file size");
                return View(employeeVM);
            }

            result = _context.Employees.Any(c => c.Name == employeeVM.Name);
            if (result)
            {
                employeeVM.Positions = await _context.Positions.ToListAsync();
                employeeVM.Departments = await _context.Departments.ToListAsync();
                ModelState.AddModelError("Name", "There is already such employee");
                return View(employeeVM);
            }

            Employee employee = new Employee
            {
                Name = employeeVM.Name,
                InstaLink = employeeVM.InstaLink,
                FbLink = employeeVM.FbLink,
                TwitterLink = employeeVM.TwitterLink,
                LinkedinLink = employeeVM.LinkedinLink,
                ImageUrl = await employeeVM.Image.CreateFile(_env.WebRootPath, "assets", "img"),
                PositionId = employeeVM.PositionId,
                DepartmentId = employeeVM.DepartmentId,
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);
            if (existed == null) return NotFound();

            var vm = new UpdateEmployeeVM
            {
                Name = existed.Name,
                DepartmentId = existed.DepartmentId,
                Departments = await _context.Departments.ToListAsync(),
                FbLink = existed.FbLink,
                TwitterLink = existed.TwitterLink,
                ImageUrl = existed.ImageUrl,
                PositionId = existed.PositionId,
                InstaLink = existed.InstaLink,
                LinkedinLink = existed.LinkedinLink,
                Positions = await _context.Positions.ToListAsync(),
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateEmployeeVM employeeVM)
        {
            var existed = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);
            if (existed == null) return NotFound();
            if(!ModelState.IsValid)
            {
                employeeVM.Positions = await _context.Positions.ToListAsync();
                employeeVM.Departments = await _context.Departments.ToListAsync();
                return View(employeeVM);
            }
            bool result = await _context.Departments.AnyAsync(c => c.Id == employeeVM.DepartmentId);
            if (!result)
            {
                employeeVM.Positions = await _context.Positions.ToListAsync();
                employeeVM.Departments = await _context.Departments.ToListAsync();
                ModelState.AddModelError("DepartmentId", "There is no such deparment");
                return View(employeeVM);
            }

            result = await _context.Positions.AnyAsync(c => c.Id == employeeVM.PositionId);
            if (!result)
            {
                employeeVM.Positions = await _context.Positions.ToListAsync();
                employeeVM.Departments = await _context.Departments.ToListAsync();
                ModelState.AddModelError("PositionId", "There is no such position");
                return View(employeeVM);
            }
            if (employeeVM.Image is not null)
            {


                if (!employeeVM.Image.ValidateType())
                {
                    employeeVM.Positions = await _context.Positions.ToListAsync();
                    employeeVM.Departments = await _context.Departments.ToListAsync();
                    ModelState.AddModelError("Image", "Wrong file type");
                    return View(employeeVM);
                }
                if (!employeeVM.Image.ValidateSize(600))
                {
                    employeeVM.Positions = await _context.Positions.ToListAsync();
                    employeeVM.Departments = await _context.Departments.ToListAsync();
                    ModelState.AddModelError("Image", "Wrong file size");
                    return View(employeeVM);
                }

                result = _context.Employees.Any(c => c.Name == employeeVM.Name && c.Id!=id);
                if (result)
                {
                    employeeVM.Positions = await _context.Positions.ToListAsync();
                    employeeVM.Departments = await _context.Departments.ToListAsync();
                    ModelState.AddModelError("Name", "There is already such employee");
                    return View(employeeVM);
                }
                existed.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "img");
                existed.ImageUrl = await employeeVM.Image.CreateFile(_env.WebRootPath, "assets", "img");

            }

            existed.Name = employeeVM.Name;
            existed.InstaLink = employeeVM.InstaLink;
            existed.FbLink = employeeVM.FbLink;
            existed.TwitterLink = employeeVM.TwitterLink;
            existed.LinkedinLink = employeeVM.LinkedinLink;
            existed.PositionId = employeeVM.PositionId;
            existed.DepartmentId = employeeVM.DepartmentId;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            var existed =  await _context.Employees.FirstOrDefaultAsync(x=>x.Id == id);
            if(existed == null) return NotFound();
            existed.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "img");
            _context.Employees.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Employees
                .Include(e=>e.Department)
                .Include(e=>e.Position)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (existed == null) return NotFound();
            return View(existed);
        }
    }
}
