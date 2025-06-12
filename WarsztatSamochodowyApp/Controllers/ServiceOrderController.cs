using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers;

public class ServiceOrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ServiceOrderController> _logger;

    public ServiceOrderController(ApplicationDbContext context, ILogger<ServiceOrderController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: ServiceOrder
    public async Task<IActionResult> Index(ServiceOrderStatus? statusFilter)
    {
        try
        {
            var query = _context.ServiceOrders.Include(v => v.Vehicle).AsQueryable();

            if (statusFilter.HasValue) query = query.Where(o => o.Status == statusFilter.Value);

            var serviceOrders = await query.ToListAsync();

            // Do widoku przekaż też listę statusów do dropdowna
            ViewData["StatusFilter"] = new SelectList(Enum.GetValues(typeof(ServiceOrderStatus))
                .Cast<ServiceOrderStatus>()
                .Select(s => new { Value = s, Text = s.ToString() }), "Value", "Text", statusFilter);

            return View(serviceOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania listy zleceń serwisowych.");
            return StatusCode(500, "Wystąpił błąd podczas pobierania zleceń.");
        }
    }


    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var serviceOrder = await _context.ServiceOrders
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceTasks)
                .ThenInclude(st => st.UsedParts)
                .ThenInclude(up => up.Part)
                .Include(o => o.Comments.OrderByDescending(c => c.CreatedAt))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceOrder == null) return NotFound();

            return View(serviceOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania szczegółów zlecenia o ID {OrderId}", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania szczegółów zlecenia.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return RedirectToAction("Details", new { id });

        try
        {
            var comment = new Comment
            {
                ServiceOrderId = id,
                Content = content,
                Author = User.Identity?.Name ?? "Anonim"
            };

            _context.ServiceOrderComments.Add(comment);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas dodawania komentarza do zlecenia o ID {OrderId}", id);
        }

        return RedirectToAction("Details", new { id });
    }

    // GET: ServiceOrder/Create
    public async Task<IActionResult> Create()
    {
        try
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            ViewData["VehicleId"] = new SelectList(vehicles, "Id", "RegistrationNumber");

            var tasks = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .ThenInclude(up => up.Part)
                .ToListAsync();

            ViewBag.ServiceTasks = tasks.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.Name} - {t.TotalCost:C}"
            }).ToList();

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przygotowywania formularza tworzenia zlecenia serwisowego.");
            return StatusCode(500, "Wystąpił błąd podczas ładowania formularza.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceOrder serviceOrder, List<int> SelectedTaskIds)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var vehicles = await _context.Vehicles.ToListAsync();
                ViewData["VehicleId"] = new SelectList(vehicles, "Id", "RegistrationNumber", serviceOrder.VehicleId);

                var tasks = await _context.ServiceTasks
                    .Include(t => t.UsedParts)
                    .ThenInclude(up => up.Part)
                    .ToListAsync();

                ViewBag.ServiceTasks = tasks.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"{t.Name} - {t.TotalCost:C}"
                }).ToList();

                return View(serviceOrder);
            }

            serviceOrder.OrderDate = DateTime.Now;
            serviceOrder.Status = ServiceOrderStatus.Nowe;

            var selectedTasks = await _context.ServiceTasks
                .Where(t => SelectedTaskIds.Contains(t.Id))
                .ToListAsync();

            serviceOrder.ServiceTasks = selectedTasks;

            _context.Add(serviceOrder);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia zlecenia serwisowego.");
            return StatusCode(500, "Wystąpił błąd podczas zapisu zlecenia.");
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var serviceOrder = await _context.ServiceOrders
                .Include(o => o.ServiceTasks)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (serviceOrder == null) return NotFound();

            ViewData["VehicleId"] =
                new SelectList(_context.Vehicles, "Id", "RegistrationNumber", serviceOrder.VehicleId);

            var allTasks = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .ThenInclude(up => up.Part)
                .ToListAsync();

            ViewBag.ServiceTasks = allTasks.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.Name} - {t.TotalCost:C}",
                Selected = serviceOrder.ServiceTasks.Any(st => st.Id == t.Id)
            }).ToList();

            return View(serviceOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przygotowywania edycji zlecenia o ID {OrderId}", id);
            return StatusCode(500, "Wystąpił błąd podczas edycji zlecenia.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceOrder serviceOrder, List<int> SelectedTaskIds)
    {
        if (id != serviceOrder.Id) return NotFound();

        try
        {
            if (ModelState.IsValid)
            {
                var existingOrder = await _context.ServiceOrders
                    .Include(o => o.ServiceTasks)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (existingOrder == null) return NotFound();

                existingOrder.Status = serviceOrder.Status;
                existingOrder.VehicleId = serviceOrder.VehicleId;

                if (serviceOrder.Status == ServiceOrderStatus.Zakonczone && existingOrder.FinishedDate == null)
                    existingOrder.FinishedDate = DateTime.Now;
                else if (serviceOrder.Status != ServiceOrderStatus.Zakonczone)
                    existingOrder.FinishedDate = null;

                existingOrder.ServiceTasks.Clear();

                var selectedTasks = await _context.ServiceTasks
                    .Where(t => SelectedTaskIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var task in selectedTasks) existingOrder.ServiceTasks.Add(task);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Błąd współbieżności przy edycji zlecenia o ID {OrderId}", id);
            if (!ServiceOrderExists(serviceOrder.Id)) return NotFound();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas edycji zlecenia o ID {OrderId}", id);
            return StatusCode(500, "Wystąpił błąd podczas zapisu zmian.");
        }

        var vehicles = await _context.Vehicles.ToListAsync();
        ViewData["VehicleId"] = new SelectList(vehicles, "Id", "RegistrationNumber", serviceOrder.VehicleId);

        var allTasks = await _context.ServiceTasks
            .Include(t => t.UsedParts)
            .ThenInclude(up => up.Part)
            .ToListAsync();

        ViewBag.ServiceTasks = allTasks.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = $"{t.Name} - {t.TotalCost:C}",
            Selected = SelectedTaskIds.Contains(t.Id)
        }).ToList();

        return View(serviceOrder);
    }

    // GET: ServiceOrder/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var serviceOrder = await _context.ServiceOrders
                .Include(v => v.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceOrder == null) return NotFound();

            return View(serviceOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas ładowania zlecenia do usunięcia o ID {OrderId}", id);
            return StatusCode(500, "Wystąpił błąd podczas ładowania zlecenia.");
        }
    }

    [HttpPost]
    [ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var serviceOrder = await _context.ServiceOrders.FindAsync(id);
            if (serviceOrder != null)
            {
                _context.ServiceOrders.Remove(serviceOrder);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania zlecenia o ID {OrderId}", id);
            return StatusCode(500, "Wystąpił błąd podczas usuwania zlecenia.");
        }
    }

    private bool ServiceOrderExists(int id)
    {
        return _context.ServiceOrders.Any(e => e.Id == id);
    }
}