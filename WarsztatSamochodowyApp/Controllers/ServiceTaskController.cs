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
    public class ServiceTaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceTaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ServiceTask
        public async Task<IActionResult> Index()
        {
            return View(await _context.ServiceTasks.ToListAsync());
        }

        // GET: ServiceTask/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceTask = await _context.ServiceTasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceTask == null)
            {
                return NotFound();
            }

            return View(serviceTask);
        }

        // GET: ServiceTask/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ServiceTask/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price")] ServiceTask serviceTask)
        {
            if (ModelState.IsValid)
            {
                _context.Add(serviceTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(serviceTask);
        }

        // GET: ServiceTask/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceTask = await _context.ServiceTasks.FindAsync(id);
            if (serviceTask == null)
            {
                return NotFound();
            }
            return View(serviceTask);
        }

        // POST: ServiceTask/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price")] ServiceTask serviceTask)
        {
            if (id != serviceTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceTaskExists(serviceTask.Id))
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
            return View(serviceTask);
        }

        // GET: ServiceTask/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceTask = await _context.ServiceTasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceTask == null)
            {
                return NotFound();
            }

            return View(serviceTask);
        }

        // POST: ServiceTask/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceTask = await _context.ServiceTasks.FindAsync(id);
            if (serviceTask != null)
            {
                _context.ServiceTasks.Remove(serviceTask);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceTaskExists(int id)
        {
            return _context.ServiceTasks.Any(e => e.Id == id);
        }
    }
}
