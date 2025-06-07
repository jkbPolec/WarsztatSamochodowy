using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers;

public class ServiceOrderController : Controller {

    private readonly ApplicationDbContext _context;

    public ServiceOrderController(ApplicationDbContext context) {
        _context = context;
    }

    // GET: ServiceOrder
    public async Task<IActionResult> Index()
    {
        var serviceOrders = await _context.ServiceOrders.Include(v => v.Vehicle).ToListAsync();
        return View(serviceOrders);
    }
    // GET: ServiceOrder/details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) return NotFound();

        var serviceOrder = await _context.ServiceOrders
            .Include(v => v.Vehicle)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (serviceOrder == null) return NotFound();

        return View(serviceOrder);
    }

    // GET: ServiceOrder/Create
    public async Task<IActionResult> Create() {
        var vehicles = await _context.Vehicles.ToListAsync();
        ViewData["VehicleId"] = new SelectList(vehicles, "Id", "RegistrationNumber");
        return View();
    }

    // POST: ServiceOrder/Create

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceOrder serviceOrder)
    {
        // DLA DEBUGOWANIA – zobacz czy VehicleId ma wartość
        Console.WriteLine($"VehicleId z formularza: {serviceOrder.VehicleId}");

        if (!ModelState.IsValid)
        {
            // Loguj błędy (dla pewności)
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                foreach (var error in state.Errors)
                {
                    Console.WriteLine($"Model error in '{key}': {error.ErrorMessage}");
                }
            }

            // Ponowne załadowanie listy pojazdów
            var vehicles = await _context.Vehicles.ToListAsync();
            ViewData["VehicleId"] = new SelectList(vehicles, "Id", "RegistrationNumber", serviceOrder.VehicleId);
            return View(serviceOrder);
        }

        _context.Add(serviceOrder);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: ServiceOrder/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) return NotFound();

        var serviceOrder = await _context.ServiceOrders.FindAsync(id);
        if (serviceOrder == null) return NotFound();

        ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber");
        return View(serviceOrder);
    }

    // POST: ServiceOrder/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceOrder serviceOrder) {
        if (id != serviceOrder.Id) {
            return NotFound();
        }

        if (ModelState.IsValid) {
            try {
                _context.Update(serviceOrder);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) {
                if (!ServiceOrderExists(serviceOrder.Id)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Jeśli masz w widoku dropdown na Vehicle, trzeba przygotować SelectList
        var vehicles = await _context.Vehicles
            .Select(v => new SelectListItem {
                Value = v.Id.ToString(),
                Text = $"{v.Vin} ({v.RegistrationNumber})"
            })
            .ToListAsync();

        ViewData["VehicleId"] = new SelectList(vehicles, "Value", "Text", serviceOrder.VehicleId);

        return View(serviceOrder);
    }

    private bool ServiceOrderExists(int id) {
        return _context.ServiceOrders.Any(e => e.Id == id);
    }

    // GET: ServiceOrder/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var vehicle = await _context.ServiceOrders
            .Include(v => v.Vehicle)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (vehicle == null) return NotFound();

        return View(vehicle);
    }
    
    // POST: ServiceOrder/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (id == null) return NotFound();
        
        var serviceOrder = await _context.ServiceOrders.FindAsync(id);
        if (serviceOrder != null) {
            _context.ServiceOrders.Remove(serviceOrder);
            await _context.SaveChangesAsync();
        }
        
        return RedirectToAction(nameof(Index));
    }
}