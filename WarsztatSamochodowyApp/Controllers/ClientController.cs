using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers;

public class ClientController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientController> _logger;

    public ClientController(ApplicationDbContext context, ILogger<ClientController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Client
    public async Task<IActionResult> Index()
    {
        try
        {
            return View(await _context.Clients.ToListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania listy klientów");
            return StatusCode(500, "Wystąpił błąd podczas pobierania danych.");
        }
    }

    // GET: Client/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null) return NotFound();

            return View(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania szczegółów klienta o Id={ClientId}", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania danych.");
        }
    }

    // GET: Client/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Client/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Email,PhoneNumber")] Client client)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia klienta");
                return StatusCode(500, "Wystąpił błąd podczas zapisywania danych.");
            }

            return RedirectToAction(nameof(Index));
        }

        return View(client);
    }

    // GET: Client/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania klienta do edycji o Id={ClientId}", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania danych.");
        }
    }

    // POST: Client/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,PhoneNumber")] Client client)
    {
        if (id != client.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Błąd współbieżności podczas edycji klienta o Id={ClientId}", client.Id);
                if (!ClientExists(client.Id)) return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(client);
    }

    // GET: Client/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null) return NotFound();

            return View(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania klienta do usunięcia o Id={ClientId}", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania danych.");
        }
    }

    // POST: Client/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania klienta o Id={ClientId}", id);
            return StatusCode(500, "Wystąpił błąd podczas usuwania klienta.");
        }

        return RedirectToAction(nameof(Index));
    }

    private bool ClientExists(int id)
    {
        return _context.Clients.Any(e => e.Id == id);
    }
}