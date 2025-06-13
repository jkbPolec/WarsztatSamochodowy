using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.DTO;

namespace WarsztatSamochodowyApp.Controllers;

[Authorize(Policy = "ClientsPolicy")]
public class ClientController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientController> _logger;
    private readonly ClientMapper _mapper;

    public ClientController(ApplicationDbContext context, ClientMapper mapper, ILogger<ClientController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: Client
    public async Task<IActionResult> Index()
    {
        try
        {
            var clients = await _context.Clients.ToListAsync();
            var dtos = clients.Select(c => _mapper.ToDto(c)).ToList();
            return View(dtos);
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
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            var dto = _mapper.ToDto(client);
            return View(dto);
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
        return View(new ClientDto());
    }

    // POST: Client/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientDto dto)
    {
        if (ModelState.IsValid)
            try
            {
                var client = _mapper.ToEntity(dto);
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia klienta");
                return StatusCode(500, "Wystąpił błąd podczas zapisywania danych.");
            }

        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            _logger.LogError("Błąd walidacji podczas tworzenia klienta: {ErrorMessage}", error.ErrorMessage);

        return View(dto);
    }

    // GET: Client/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            var dto = _mapper.ToDto(client);
            return View(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania klienta do edycji o Id={ClientId}", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania danych.");
        }
    }

    // POST: Client/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClientDto dto)
    {
        if (id != dto.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            _mapper.UpdateEntity(dto, client);

            _context.Update(client);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(dto);
    }

    // GET: Client/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            var dto = _mapper.ToDto(client);
            return View(dto);
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
        var client = await _context.Clients.FindAsync(id);
        if (client != null)
        {
            _context.Clients.Remove(client);
            _logger.LogInformation("Usunięto klienta o Id={ClientId}", id);
        }
        else
        {
            _logger.LogWarning("Nie znaleziono klienta do usunięcia o Id={ClientId}", id);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ClientExists(int id)
    {
        return _context.Clients.Any(e => e.Id == id);
    }
}