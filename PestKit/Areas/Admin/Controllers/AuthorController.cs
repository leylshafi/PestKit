using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PestKit.Areas.Admin.ViewModels;
using PestKit.Data;
using PestKit.Models;

namespace PestKit.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthorController : Controller
    {
        private readonly AppDbContext _context;

		public AuthorController(AppDbContext context)
		{
			_context = context;
		}
		[Authorize(Roles ="Admin,Moderator")]
        public async Task<IActionResult> Index(int page)
        {
			double count = await _context.Authors.CountAsync();
			var authors = await _context.Authors.Skip(page * 2).Take(2).Include(a => a.Blogs).ToListAsync();
			PaginationVM<Author> pagination = new()
			{
				CurrentPage = page,
				TotalPage = Math.Ceiling(count/2),
				Items = authors
			};
            return View(pagination);
        }
		[Authorize(Roles = "Admin,Moderator")]
		public IActionResult Create() { 
            return View();
        }

		[HttpPost]
		public async Task<IActionResult> Create(CreateAuthorVM authorVM)
		{
			if (!ModelState.IsValid) return View();
			var author = new Author
			{
				Name = authorVM.Name,
				Surname = authorVM.Surname,
			};
			bool result = _context.Authors.Any(c => c.Name.Trim() == author.Name.Trim() && c.Surname.Trim() == author.Surname.Trim());
			if (result)
			{
				ModelState.AddModelError("Name", "This author already exists");
				return View();
			}
			await _context.Authors.AddAsync(author);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		[Authorize(Roles = "Admin,Moderator")]
		public async Task<IActionResult> Update(int id)
		{
			if (id <= 0) return BadRequest();
			Author author = await _context.Authors.FirstOrDefaultAsync(c => c.Id == id);

			if (author is null) return NotFound();

			return View(new UpdateAuthorVM
			{
				Name = author.Name,
				Surname = author.Surname,
			});
		}

		[HttpPost]
		public async Task<IActionResult> Update(int id, UpdateAuthorVM authorVM)
		{
			if (!ModelState.IsValid) return View();

			Author existed = await _context.Authors.FirstOrDefaultAsync(c => c.Id == id);
			var author = new Author
			{
				Name = authorVM.Name,
				Surname = authorVM.Surname,
			};
			if (existed is null) return NotFound();
			bool result = _context.Authors.Any(c => c.Name == author.Name && c.Surname == author.Surname && c.Id != id);
			if (result)
			{
				ModelState.AddModelError("Name", "There is already such author");
				return View();
			}

			existed.Name = author.Name;
			existed.Surname = author.Surname;
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int id)
		{
			if (id <= 0) return BadRequest();
			var existed = await _context.Authors.FirstOrDefaultAsync(c => c.Id == id);
			if (existed is null) return NotFound();
			_context.Authors.Remove(existed);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		[Authorize(Roles = "Admin,Moderator")]
		public async Task<IActionResult> Details(int id)
		{
			if (id <= 0) return BadRequest();
			var author = await _context.Authors.Include(c => c.Blogs).FirstOrDefaultAsync(s => s.Id == id);
			if (author == null) return NotFound();

			return View(author);
		}
	}
}
