using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Mappers;

namespace WarsztatSamochodowyApp.Controllers;

[Authorize(Policy = "CarPartsPolicy")]
public class PartsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PartsController> _logger;
    private readonly PartMapper _mapper;

    public PartsController(ApplicationDbContext context, PartMapper mapper, ILogger<PartsController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: Parts
    public async Task<IActionResult> Index()
    {
        try
        {
            var parts = await _context.Parts.Include(p => p.PartType).ToListAsync();
            var partDtos = parts.Select(p => _mapper.ToDto(p)).ToList();
            return View(partDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania listy części.");
            return StatusCode(500, "Wystąpił błąd podczas pobierania części.");
        }
    }

    // GET: Parts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var part = await _context.Parts
                .Include(p => p.PartType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (part == null) return NotFound();

            return View(part);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania szczegółów części o id {Id}.", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania szczegółów części.");
        }
    }

    // GET: Parts/Create
    public IActionResult Create()
    {
        try
        {
            ViewData["PartTypeId"] = new SelectList(_context.PartTypes, "Id", "Name");
            return View(new PartDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przygotowywania formularza tworzenia części.");
            return StatusCode(500, "Wystąpił błąd podczas przygotowywania formularza.");
        }
    }

    // POST: Parts/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartDto dto)
    {
        if (ModelState.IsValid)
        {
            var part = _mapper.ToEntity(dto); // mapper zainicjalizowany w konstruktorze
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            _logger.LogError("Błąd walidacji podczas tworzenia części: {ErrorMessage}", error.ErrorMessage);
            Console.WriteLine(error.ErrorMessage);
        }

        ViewData["PartTypeId"] = new SelectList(_context.PartTypes, "Id", "Name", dto.PartTypeId);
        return View(dto);
    }

    // GET: Parts/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();
            var dto = _mapper.ToDto(part);
            ViewData["PartTypeId"] = new SelectList(_context.PartTypes, "Id", "Name", dto.PartTypeId);
            return View(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przygotowywania edycji części o id {Id}.", id);
            return StatusCode(500, "Wystąpił błąd podczas przygotowywania edycji części.");
        }
    }

    // POST: Parts/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PartDto dto)
    {
        if (id != dto.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null)
            {
                _logger.LogWarning("Nie znaleziono części o id {Id} podczas edycji.", id);
                return NotFound();
            }

            _mapper.UpdateEntity(dto, part);

            _context.Update(part);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewData["PartTypeId"] = new SelectList(_context.PartTypes, "Id", "Name", dto.PartTypeId);
        return View(dto);
    }

    // GET: Parts/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var part = await _context.Parts
                .Include(p => p.PartType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (part == null) return NotFound();

            return View(part);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przygotowywania usuwania części o id {Id}.", id);
            return StatusCode(500, "Wystąpił błąd podczas przygotowywania usuwania części.");
        }
    }

    // POST: Parts/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var part = await _context.Parts.FindAsync(id);
        if (part != null)
        {
            _context.Parts.Remove(part);
            _logger.LogInformation("Usunięto część o id {Id}.", id);
        }
        else
        {
            _logger.LogWarning("Nie znaleziono części o id {Id} do usunięcia.", id);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PartExists(int id)
    {
        try
        {
            return _context.Parts.Any(e => e.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas sprawdzania istnienia części o id {Id}.", id);
            return false;
        }
    }
}