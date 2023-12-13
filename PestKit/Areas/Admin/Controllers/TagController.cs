using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PestKit.Areas.Admin.ViewModels;
using PestKit.Data;
using PestKit.Models;

namespace PestKit.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class TagController : Controller
	{
		private readonly AppDbContext _context;

		public TagController(AppDbContext context)
		{
			_context = context;
		}
		[Authorize(Roles = "Admin,Moderator")]
		public async Task<IActionResult> Index(int page)
		{
            double count = await _context.Tags.CountAsync();
			var tags = await _context.Tags.Skip(page*2).Take(2)
                .ToListAsync();
            PaginationVM<Tag> pagination = new PaginationVM<Tag>()
            {
                CurrentPage = page,
                TotalPage = Math.Ceiling(count / 2),
                Items = tags
            };
			return View(pagination);
		}
		[Authorize(Roles = "Admin,Moderator")]
		public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateTagVM tagVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool result = _context.Tags.Any(c => c.Name.ToLower().Trim() == tagVM.Name.ToLower().Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "This tag already exists");
                return View();
            }
            var tag = new Tag
            {
                Name = tagVM.Name,
            };
            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
		[Authorize(Roles = "Admin,Moderator")]
		public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            var existed =  await _context.Tags.FirstOrDefaultAsync(c => c.Id == id);
            if(existed == null) return NotFound();
            var vm = new UpdateTagVM
            {
                Name = existed.Name,
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateTagVM tagVM)
        {
            if(!ModelState.IsValid)
            {
                return View(tagVM);
            }
            var existed = await _context.Tags.FirstOrDefaultAsync(c => c.Id == id);
            if(existed == null) return NotFound();
            bool result = _context.Tags.Any(c => c.Name == tagVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "There is already such tag");
                return View(tagVM);
            }

            existed.Name = tagVM.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int id)
        {
            var existed = await _context.Tags.FirstOrDefaultAsync(c => c.Id == id);
            if (existed == null) return NotFound();
            _context.Tags.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
		[Authorize(Roles = "Admin,Moderator")]
		public async Task<IActionResult>Details(int id)
        {
            if (id <= 0) return BadRequest();
			var existed = await _context.Tags.
                Include(t=>t.BlogTags).
                ThenInclude(bt=>bt.Blog).
                FirstOrDefaultAsync(c => c.Id == id);
			if (existed == null) return NotFound();

            return View(existed);
		}
    }
}
