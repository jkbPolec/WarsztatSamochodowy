using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
            var tasks = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                    .ThenInclude(up => up.Part)
                .ToListAsync();

            return View(tasks);
        }

        // GET: ServiceTask/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var serviceTask = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                    .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (serviceTask == null) return NotFound();

            return View(serviceTask);
        }

        // GET: ServiceTask/Create
        public async Task<IActionResult> Create()
        {
            var allParts = await _context.Parts.ToListAsync();
            ViewBag.AllParts = allParts;
            return View();
        }

        // POST: ServiceTask/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceTask serviceTask, List<int> partIds, List<int> quantities)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllParts = await _context.Parts.ToListAsync();
                return View(serviceTask);
            }

            if (partIds != null && quantities != null && partIds.Count == quantities.Count)
            {
                serviceTask.UsedParts = partIds.Select((partId, i) => new UsedPart
                {
                    PartId = partId,
                    Quantity = quantities[i]
                }).Where(up => up.Quantity > 0).ToList();
            }

            _context.ServiceTasks.Add(serviceTask);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceTask/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var serviceTask = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (serviceTask == null) return NotFound();

            ViewBag.AllParts = await _context.Parts.ToListAsync();
            return View(serviceTask);
        }

        // POST: ServiceTask/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceTask serviceTask, List<int> partIds, List<int> quantities)
        {
            if (id != serviceTask.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.AllParts = await _context.Parts.ToListAsync();
                return View(serviceTask);
            }

            var existingTask = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTask == null) return NotFound();

            existingTask.Name = serviceTask.Name;
            existingTask.Description = serviceTask.Description;
            existingTask.Price = serviceTask.Price;

            // Usuń stare części
            _context.UsedParts.RemoveRange(existingTask.UsedParts);

            if (partIds != null && quantities != null && partIds.Count == quantities.Count)
            {
                existingTask.UsedParts = partIds.Select((partId, i) => new UsedPart
                {
                    PartId = partId,
                    Quantity = quantities[i]
                }).Where(up => up.Quantity > 0).ToList();
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceTask/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var serviceTask = await _context.ServiceTasks
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceTask == null) return NotFound();

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
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ServiceTaskExists(int id)
        {
            return _context.ServiceTasks.Any(e => e.Id == id);
        }
    }
}
