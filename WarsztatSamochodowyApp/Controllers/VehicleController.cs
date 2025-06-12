using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Mappers;
using WarsztatSamochodowyApp.Models;

// dodaj namespace DTO
// dodaj mapper manualny
// using WarsztatSamochodowyApp.Mappers; // jeśli tam masz VehicleMapperManual

namespace WarsztatSamochodowyApp.Controllers;

public class VehicleController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VehicleController> _logger;
    private readonly VehicleMapper _mapper;

    public VehicleController(ApplicationDbContext context, ILogger<VehicleController> logger, VehicleMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    // GET: Vehicle
    public async Task<IActionResult> Index()
    {
        try
        {
            var vehicles = await _context.Vehicles.Include(v => v.Client).ToListAsync();
            var vehicleDtos = vehicles.Select(v => _mapper.ToDto(v)).ToList();
            return View(vehicleDtos); // przekazujemy listę DTO do widoku
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania listy pojazdów.");
            return StatusCode(500, "Wystąpił błąd podczas pobierania pojazdów.");
        }
    }

    // GET: Vehicle/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var vehicle = await _context.Vehicles.Include(v => v.Client).FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null) return NotFound();

            var dto = _mapper.ToDto(vehicle);
            return View(dto); // przekazujemy DTO do widoku
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania szczegółów pojazdu o ID {VehicleId}", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania szczegółów pojazdu.");
        }
    }

    // GET: Vehicle/Create
    public IActionResult Create()
    {
        try
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "LastName");
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przygotowywania formularza tworzenia pojazdu.");
            return StatusCode(500, "Wystąpił błąd podczas przygotowywania formularza.");
        }
    }

    // POST: Vehicle/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleDto dto, IFormFile ImageFile)
    {
        try
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

                dto.ImageUrl = "/images/" + fileName;
            }

            if (ModelState.IsValid)
            {
                var vehicle = _mapper.FromDto(dto);

                // Jeśli chcesz, możesz tu załadować Client z bazy i przypisać do pojazdu:
                vehicle.Client = await _context.Clients.FindAsync(dto.ClientId);

                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia pojazdu.");
            return StatusCode(500, "Wystąpił błąd podczas tworzenia pojazdu.");
        }

        ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "LastName", dto.ClientId);
        return View(dto);
    }

    // GET: Vehicle/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var vehicle = await _context.Vehicles.Include(v => v.Client).FirstOrDefaultAsync(v => v.Id == id);
        if (vehicle == null) return NotFound();

        var dto = _mapper.ToDto(vehicle);

        ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "LastName", dto.ClientId);
        return View(dto);
    }

    // POST: Vehicle/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VehicleDto dto)
    {
        if (id != dto.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var vehicle = _mapper.FromDto(dto);
                vehicle.Client = await _context.Clients.FindAsync(dto.ClientId);

                _context.Update(vehicle);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Błąd współbieżności podczas edycji pojazdu o ID {VehicleId}", id);
                if (!_context.Vehicles.Any(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "LastName", dto.ClientId);
        return View(dto);
    }

    // GET: Vehicle/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var vehicle = await _context.Vehicles.Include(v => v.Client).FirstOrDefaultAsync(m => m.Id == id);
        if (vehicle == null) return NotFound();

        var dto = _mapper.ToDto(vehicle);
        return View(dto);
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
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania pojazdu o ID {VehicleId}", id);
                throw;
            }
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
            ClientId = 2
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Dodano pojazd na sztywno: VIN={Vin}, Rejestracja={RegNum}, KlientId={ClientId}",
            vehicle.Vin, vehicle.RegistrationNumber, vehicle.ClientId);

        return Content("Pojazd dodany na sztywno!");
    }
}