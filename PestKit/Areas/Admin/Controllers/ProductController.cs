using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PestKit.Areas.Admin.ViewModels;
using PestKit.Data;
using PestKit.Models;

namespace PestKit.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }
		[Authorize(Roles = "Admin,Moderator")]
		public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Products.CountAsync();
            var products = await _context.Products.Skip(page*2).Take(2).ToListAsync();
            PaginationVM<Product> pagination = new()
            {
                TotalPage = Math.Ceiling(count/2),
                CurrentPage = page,
                Items = products
            };
            return View(pagination);
        }
		[Authorize(Roles = "Admin,Moderator")]
		public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            if(!ModelState.IsValid) return View();
            bool result = await _context.Products.AnyAsync(p=>p.Name==productVM.Name);
            if(result)
            {
                ModelState.AddModelError("Name", "This product already exists");
                return View();
            }

            Product product = new Product()
            {
                Name = productVM.Name,
                Price = productVM.Price,
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int id)
        {
            if(id<=0) return BadRequest();
            var existed = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();
            _context.Products.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles ="Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();
            var vm = new UpdateProductVM
            {
                Name = existed.Name,
                Price = existed.Price,
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
        {
            if (!ModelState.IsValid) return View(productVM);
            var existed = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();

            bool result = _context.Products.Any(c => c.Name == productVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "There is already such product");
                return View();
            }

            existed.Name = productVM.Name;
            existed.Price = productVM.Price;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        [Authorize(Roles ="Admin,Moderator")]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Products.FirstOrDefaultAsync(p=>p.Id == id);
            if (existed is null) return NotFound();
            return View(existed);

        }
    }
}
