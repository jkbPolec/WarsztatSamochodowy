using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers;

[Authorize(Policy = "CarPartsPolicy")]
public class PartTypeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PartTypeController> _logger;

    public PartTypeController(ApplicationDbContext context, ILogger<PartTypeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: PartType
    public async Task<IActionResult> Index()
    {
        try
        {
            return View(await _context.PartTypes.ToListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania listy PartTypes");
            return StatusCode(500, "Wystąpił błąd podczas pobierania danych.");
        }
    }

    // GET: PartType/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        try
        {
            if (id == null) return NotFound();

            var partType = await _context.PartTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (partType == null) return NotFound();

            return View(partType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania szczegółów PartType o Id={PartTypeId}", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania danych.");
        }
    }

    // GET: PartType/Create
    public IActionResult Create()
    {
        try
        {
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wyświetlania formularza tworzenia PartType");
            return StatusCode(500, "Wystąpił błąd podczas ładowania formularza.");
        }
    }

    // POST: PartType/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name")] PartType partType)
    {
        if (ModelState.IsValid)
        {
            _context.Add(partType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("Nieprawidłowy model podczas tworzenia PartType");
        return View(partType);
    }

    // GET: PartType/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        try
        {
            if (id == null) return NotFound();

            var partType = await _context.PartTypes.FindAsync(id);
            if (partType == null) return NotFound();
            return View(partType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania PartType do edycji o Id={PartTypeId}", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania danych.");
        }
    }

    // POST: PartType/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] PartType partType)
    {
        if (id != partType.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(partType);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex,
                    "Błąd współbieżności podczas edycji PartType o Id={PartTypeId}", partType.Id);
                if (!PartTypeExists(partType.Id)) return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("Nieprawidłowy model podczas edycji PartType o Id={PartTypeId}",
            partType.Id);
        return View(partType);
    }

    // GET: PartType/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        try
        {
            if (id == null) return NotFound();

            var partType = await _context.PartTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (partType == null) return NotFound();

            return View(partType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Błąd podczas pobierania PartType do usunięcia o Id={PartTypeId}", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania danych.");
        }
    }

    // POST: PartType/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var partType = await _context.PartTypes.FindAsync(id);
        if (partType == null)
            _logger.LogWarning("Próba usunięcia nieistniejącego PartType o Id={PartTypeId}",
                id);
        else
            _context.PartTypes.Remove(partType);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania PartType o Id={PartTypeId}", id);
            return StatusCode(500, "Wystąpił błąd podczas usuwania danych.");
        }

        return RedirectToAction(nameof(Index));
    }

    private bool PartTypeExists(int id)
    {
        return _context.PartTypes.Any(e => e.Id == id);
    }
}