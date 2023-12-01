﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PestKit.Areas.Admin.ViewModels;
using PestKit.Data;
using PestKit.Models;

namespace PestKit.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class PositionController : Controller
	{
		private readonly AppDbContext _context;

		public PositionController(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var positions = await _context.Positions.Include(p=>p.Employees).ToListAsync();
			return View(positions);
		}

		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(CreatePositionVM positionVM)
		{
			if(!ModelState.IsValid) return View(positionVM);
            bool result = _context.Positions.Any(c => c.Name == positionVM.Name);
            if (result)
            {
                ModelState.AddModelError("Name", "There is already such position");
                return View();
            }

			var position = new Position
			{
				Name = positionVM.Name,
			};
			await _context.Positions.AddAsync(position);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
        }

		public async Task<IActionResult> Update(int id)
		{
			if (id <= 0) return BadRequest();
			var existed = await _context.Positions.FirstOrDefaultAsync(p=>p.Id == id);
			if (existed is null) return NotFound();
			var vm = new UpdatePositionVM
			{
				Name = existed.Name
			};
			return View(vm);
		}

		[HttpPost]
        public async Task<IActionResult> Update(int id,UpdatePositionVM positionVM)
        {
			if(!ModelState.IsValid) return View(positionVM);
			var existed =  await _context.Positions.FirstOrDefaultAsync(p=>p.Id==id);
			if(existed is null) return NotFound();

            bool result = _context.Positions.Any(c => c.Name == positionVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "There is already such position");
                return View();
            }
			
            existed.Name = positionVM.Name;
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));

        }

		public async Task<IActionResult> Delete(int id)
		{
            if (id <= 0) return BadRequest();
            var existed = await _context.Positions.FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();
			_context.Positions.Remove(existed);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
        }

		public async Task<IActionResult> Details(int id)
		{
            if (id <= 0) return BadRequest();
            var existed = await _context.Positions.Include(p=>p.Employees).FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();
			return View(existed);
        }

    }
}