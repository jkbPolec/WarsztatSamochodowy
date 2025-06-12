using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers;

public class ServiceTaskController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ServiceTaskController> _logger;

    public ServiceTaskController(ApplicationDbContext context, ILogger<ServiceTaskController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var tasks = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .ThenInclude(up => up.Part)
                .ToListAsync();

            return View(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania listy zadań serwisowych.");
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var serviceTask = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (serviceTask == null) return NotFound();

            return View(serviceTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania szczegółów zadania serwisowego o ID {TaskId}", id);
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var allParts = await _context.Parts.ToListAsync();
            ViewBag.AllParts = allParts;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przygotowywania formularza tworzenia zadania serwisowego.");
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceTask serviceTask, List<int> partIds, List<int> quantities)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AllParts = await _context.Parts.ToListAsync();
            return View(serviceTask);
        }

        try
        {
            if (partIds != null && quantities != null && partIds.Count == quantities.Count)
                serviceTask.UsedParts = partIds.Select((partId, i) => new UsedPart
                {
                    PartId = partId,
                    Quantity = quantities[i]
                }).Where(up => up.Quantity > 0).ToList();

            _context.ServiceTasks.Add(serviceTask);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia zadania serwisowego.");
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var serviceTask = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (serviceTask == null) return NotFound();

            ViewBag.AllParts = await _context.Parts.ToListAsync();
            return View(serviceTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przygotowywania edycji zadania o ID {TaskId}", id);
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

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

        try
        {
            var existingTask = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTask == null) return NotFound();

            existingTask.Name = serviceTask.Name;
            existingTask.Description = serviceTask.Description;
            existingTask.Price = serviceTask.Price;

            _context.UsedParts.RemoveRange(existingTask.UsedParts);

            if (partIds != null && quantities != null && partIds.Count == quantities.Count)
                existingTask.UsedParts = partIds.Select((partId, i) => new UsedPart
                {
                    PartId = partId,
                    Quantity = quantities[i]
                }).Where(up => up.Quantity > 0).ToList();

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas edycji zadania serwisowego o ID {TaskId}", id);
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var serviceTask = await _context.ServiceTasks
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceTask == null) return NotFound();

            return View(serviceTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas ładowania zadania do usunięcia o ID {TaskId}", id);
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var serviceTask = await _context.ServiceTasks.FindAsync(id);
            if (serviceTask != null)
            {
                _context.ServiceTasks.Remove(serviceTask);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania zadania serwisowego o ID {TaskId}", id);
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    private bool ServiceTaskExists(int id)
    {
        return _context.ServiceTasks.Any(e => e.Id == id);
    }
}