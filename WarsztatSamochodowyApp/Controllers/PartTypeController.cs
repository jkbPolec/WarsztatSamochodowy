using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers
{
    public class PartTypeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PartTypeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PartType
        public async Task<IActionResult> Index()
        {
            return View(await _context.PartTypes.ToListAsync());
        }

        // GET: PartType/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partType = await _context.PartTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (partType == null)
            {
                return NotFound();
            }

            return View(partType);
        }

        // GET: PartType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PartType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] PartType partType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(partType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(partType);
        }

        // GET: PartType/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partType = await _context.PartTypes.FindAsync(id);
            if (partType == null)
            {
                return NotFound();
            }
            return View(partType);
        }

        // POST: PartType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] PartType partType)
        {
            if (id != partType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(partType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartTypeExists(partType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(partType);
        }

        // GET: PartType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partType = await _context.PartTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (partType == null)
            {
                return NotFound();
            }

            return View(partType);
        }

        // POST: PartType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var partType = await _context.PartTypes.FindAsync(id);
            if (partType != null)
            {
                _context.PartTypes.Remove(partType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PartTypeExists(int id)
        {
            return _context.PartTypes.Any(e => e.Id == id);
        }
    }
}
