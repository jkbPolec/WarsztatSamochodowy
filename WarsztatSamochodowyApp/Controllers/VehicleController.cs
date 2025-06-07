using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers;

public class VehicleController : Controller
{
    private readonly ApplicationDbContext _context;

    public VehicleController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Vehicle
    public async Task<IActionResult> Index()
    {
        var vehicles = _context.Vehicles.Include(v => v.Client);
        return View(await vehicles.ToListAsync());
    }

    // GET: Vehicle/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var vehicle = await _context.Vehicles
            .Include(v => v.Client)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (vehicle == null) return NotFound();

        return View(vehicle);
    }

    // GET: Vehicle/Create
    public IActionResult Create()
    {
        // Poprawiamy listę wyboru na bardziej czytelną (np. LastName klienta)
        ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "LastName");
        return View();
    }

    // POST: Vehicle/Create

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Vehicle vehicle, IFormFile ImageFile)
    {
        if (ImageFile != null && ImageFile.Length > 0)
        {
            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            vehicle.ImageUrl = "/images/" + fileName; // zapisz ścieżkę do bazy
        }

        if (ModelState.IsValid)
        {
            _context.Add(vehicle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["ClientId"] = new SelectList(_context.Set<Client>(), "Id", "LastName", vehicle.ClientId);
        return View(vehicle);
    }

    // Reszta akcji (Edit, Delete) bez zmian...

    private bool VehicleExists(int id)
    {
        return _context.Vehicles.Any(e => e.Id == id);
    }

    // GET: Vehicle/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();

        ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "LastName", vehicle.ClientId);
        return View(vehicle);
    }

// POST: Vehicle/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Vehicle vehicle)
    {
        if (id != vehicle.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(vehicle);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Vehicles.Any(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "LastName", vehicle.ClientId);
        return View(vehicle);
    }

// GET: Vehicle/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var vehicle = await _context.Vehicles
            .Include(v => v.Client)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (vehicle == null) return NotFound();

        return View(vehicle);
    }

// POST: Vehicle/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle != null)
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> AddFixedVehicle()
    {
        var vehicle = new Vehicle
        {
            Vin = "1234567890VIN",
            RegistrationNumber = "ABC1234",
            ImageUrl = "https://example.com/car.jpg",
            ClientId = 2 // podaj tutaj istniejące Id klienta z bazy
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return Content("Pojazd dodany na sztywno!");
    }
}