using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers;

public class ServiceOrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public ServiceOrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: ServiceOrder
    public async Task<IActionResult> Index()
    {
        var serviceOrders = await _context.ServiceOrders.Include(v => v.Vehicle).ToListAsync();
        return View(serviceOrders);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return RedirectToAction("Details", new { id });

        var comment = new Comment
        {
            ServiceOrderId = id,
            Content = content,
            Author = User.Identity?.Name ?? "Anonim"
        };

        _context.ServiceOrderComments.Add(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id });
    }

    // GET: ServiceOrder/Create
    public async Task<IActionResult> Create()
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

    // POST: ServiceOrder/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceOrder serviceOrder, List<int> SelectedTaskIds)
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
        serviceOrder.Status = ServiceOrderStatus.Nowe;  // <-- ustawiamy automatycznie status na "Nowe"

        var selectedTasks = await _context.ServiceTasks
            .Where(t => SelectedTaskIds.Contains(t.Id))
            .ToListAsync();

        serviceOrder.ServiceTasks = selectedTasks;

        _context.Add(serviceOrder);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: ServiceOrder/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var serviceOrder = await _context.ServiceOrders
            .Include(o => o.ServiceTasks)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (serviceOrder == null) return NotFound();

        ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber", serviceOrder.VehicleId);

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

    // POST: ServiceOrder/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceOrder serviceOrder, List<int> SelectedTaskIds)
    {
        if (id != serviceOrder.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existingOrder = await _context.ServiceOrders
                    .Include(o => o.ServiceTasks)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (existingOrder == null) return NotFound();

                existingOrder.OrderDate = serviceOrder.OrderDate;
                existingOrder.Status = serviceOrder.Status;
                if (serviceOrder.Status == ServiceOrderStatus.Zakonczone && existingOrder.FinishedDate == null)
                {
                    existingOrder.FinishedDate = DateTime.Now;
                }
                else if (serviceOrder.Status != ServiceOrderStatus.Zakonczone)
                {
                    existingOrder.FinishedDate = null;
                }
                existingOrder.VehicleId = serviceOrder.VehicleId;

                existingOrder.ServiceTasks.Clear();

                var selectedTasks = await _context.ServiceTasks
                    .Where(t => SelectedTaskIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var task in selectedTasks)
                {
                    existingOrder.ServiceTasks.Add(task);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceOrderExists(serviceOrder.Id)) return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
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

    private bool ServiceOrderExists(int id)
    {
        return _context.ServiceOrders.Any(e => e.Id == id);
    }

    // GET: ServiceOrder/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var serviceOrder = await _context.ServiceOrders
            .Include(v => v.Vehicle)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (serviceOrder == null) return NotFound();

        return View(serviceOrder);
    }

    // POST: ServiceOrder/Delete/5
    [HttpPost]
    [ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var serviceOrder = await _context.ServiceOrders.FindAsync(id);
        if (serviceOrder != null)
        {
            _context.ServiceOrders.Remove(serviceOrder);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
