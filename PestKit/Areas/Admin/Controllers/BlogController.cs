using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PestKit.Areas.Admin.ViewModels;
using PestKit.Data;
using PestKit.Models;
using PestKit.Utilities.Extetions;

namespace PestKit.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class BlogController : Controller
	{
		private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _env;

		public BlogController(AppDbContext context, IWebHostEnvironment env)
		{
			_context = context;
			_env = env;
		}

		public async Task<IActionResult> Index()
		{
			return View(await _context.Blogs.Include(b=>b.Author).ToListAsync());
		}

		public async Task<IActionResult> Create()
		{
			CreateBlogVM vm = new CreateBlogVM()
			{
				Tags = await _context.Tags.ToListAsync(),
				Authors  = await _context.Authors.ToListAsync(),
			};
			return View(vm);
		}
		[HttpPost]
		public async Task<IActionResult> Create(CreateBlogVM blogVM)
		{
			if(!ModelState.IsValid)
			{
				blogVM.Tags = await _context.Tags.ToListAsync();
				blogVM.Authors = await _context.Authors.ToListAsync();
				return View(blogVM);
			}
			if (!blogVM.Photo.ValidateType())
			{
				ModelState.AddModelError("Photo", "Wrong file type");
				return View();
			}
			if (!blogVM.Photo.ValidateSize(2 * 1024))
			{
				ModelState.AddModelError("Photo", "It shouldn't exceed 2 mb");
				return View();
			}
			
			var blog = new Blog
			{
				Name = blogVM.Name,
				Description = blogVM.Description,
				ImageUrl = await blogVM.Photo.CreateFile(_env.WebRootPath, "assets", "img"),
				AuthorId = blogVM.AuthorId,
				BlogTags = new()
			};
            foreach (int tId in blogVM.TagIds)
            {
				var blogTag = new BlogTag
				{
					TagId = tId,
					Blog = blog,
				};
                blog.BlogTags.Add(blogTag);
            }

            await _context.Blogs.AddAsync(blog);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int id)
		{
			var existed = await _context.Blogs.FirstOrDefaultAsync(c => c.Id == id);
			if (existed == null) return NotFound();
			_context.Blogs.Remove(existed);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Update(int id)
		{
			if(id<=0) return BadRequest();
			var existed = await _context.Blogs
				.Include(b=>b.BlogTags)
				.FirstOrDefaultAsync(c => c.Id == id);
			if(existed == null) return NotFound();
			var vm = new UpdateBlogVM()
			{
				Name = existed.Name,
				AuthorId = existed.AuthorId,
				Description = existed.Description,
				ImageUrl = existed.ImageUrl,
				Authors = await _context.Authors.ToListAsync(),
				Tags = await _context.Tags.ToListAsync(),
				TagIds = existed.BlogTags.Select(pt => pt.TagId).ToList(),
				
			};
			return View(vm);
		}
		[HttpPost]
		public async Task<IActionResult> Update(int id, UpdateBlogVM vm)
		{
			if (!ModelState.IsValid)
			{
				vm.Authors = await _context.Authors.ToListAsync();
				vm.Tags = await _context.Tags.ToListAsync();
				return View(vm);
			}
			if (!vm.Photo.ValidateType())
			{
				ModelState.AddModelError("Photo", "Wrong file type");
				return View();
			}
			if (!vm.Photo.ValidateSize(2 * 1024))
			{
				ModelState.AddModelError("Photo", "It shouldn't exceed 2 mb");
				return View();
			}

			Blog existed = await _context.Blogs
				.Include(b=>b.BlogTags)
				.Include(b=>b.Author)
				.FirstOrDefaultAsync(b => b.Id == id);

			if(existed == null) return NotFound();
			bool result = await _context.Blogs.AnyAsync(c => c.AuthorId == vm.AuthorId);
			if (!result)
			{
				vm.Authors = await _context.Authors.ToListAsync();
				vm.Tags = await _context.Tags.ToListAsync();
				return View(vm);
			}

			foreach (int idT in vm.TagIds)
			{
				bool TagResult = await _context.Tags.AnyAsync(t => t.Id == idT);
				if (!TagResult)
				{
					vm.Authors = await _context.Authors.ToListAsync();
					vm.Tags = await _context.Tags.ToListAsync();
					ModelState.AddModelError("TagIds", "There is no such tag");
					return View(vm);
				}
			}

			result = _context.Blogs.Any(c => c.Name == vm.Name && c.Id != id);
			if (result)
			{
				vm.Authors = await _context.Authors.ToListAsync();
				vm.Tags = await _context.Tags.ToListAsync();
				ModelState.AddModelError("Name", "There is already such blog");
				return View(vm);
			}

			existed.BlogTags.RemoveAll(pTag => !vm.TagIds.Contains(pTag.Id));

			existed.BlogTags.AddRange(vm.TagIds.Where(tagId => !existed.BlogTags.Any(pt => pt.Id == tagId))
								 .Select(tagId => new BlogTag { TagId = tagId }));

			existed.Name = vm.Name;
			existed.Description = vm.Description;
			existed.ImageUrl = await vm.Photo.CreateFile(_env.WebRootPath, "assets", "img");
			existed.AuthorId = vm.AuthorId;
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Details(int id)
		{
			if (id <= 0) return BadRequest();
			var existed = await _context.Blogs.
				Include(t => t.BlogTags).
				ThenInclude(bt => bt.Tag).
				Include(t=>t.Author).
				FirstOrDefaultAsync(c => c.Id == id);
			if (existed == null) return NotFound();

			return View(existed);
		}
	}
}
