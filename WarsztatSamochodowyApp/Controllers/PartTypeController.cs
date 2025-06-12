using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Mappers;

namespace WarsztatSamochodowyApp.Controllers;

[Authorize(Policy = "CarPartsPolicy")]
public class PartTypeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PartTypeController> _logger;
    private readonly PartMapper _mapper;

    public PartTypeController(ApplicationDbContext context, PartMapper mapper, ILogger<PartTypeController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: PartType
    public async Task<IActionResult> Index()
    {
        try
        {
            var entities = await _context.PartTypes.ToListAsync();
            var dtos = entities.Select(_mapper.ToDto).ToList();
            return View(dtos);
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
        if (id == null) return NotFound();

        try
        {
            var entity = await _context.PartTypes.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null) return NotFound();

            var dto = _mapper.ToDto(entity);
            return View(dto);
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
            return View(new PartTypeDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wyświetlania formularza tworzenia PartType");
            return StatusCode(500, "Wystąpił błąd podczas ładowania formularza.");
        }
    }

    // POST: PartType/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartTypeDto dto)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.ToEntity(dto);
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("Nieprawidłowy model podczas tworzenia PartType");
        return View(dto);
    }

    // GET: PartType/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var entity = await _context.PartTypes.FindAsync(id);
            if (entity == null) return NotFound();

            var dto = _mapper.ToDto(entity);
            return View(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania PartType do edycji o Id={PartTypeId}", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania danych.");
        }
    }

    // POST: PartType/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PartTypeDto dto)
    {
        if (id != dto.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var entity = await _context.PartTypes.FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("Nie znaleziono PartType o Id={PartTypeId} podczas edycji.", id);
                return NotFound();
            }

            _mapper.UpdateEntity(dto, entity);

            _context.Update(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("Nieprawidłowy model podczas edycji PartType o Id={PartTypeId}", dto.Id);
        return View(dto);
    }

    // GET: PartType/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var entity = await _context.PartTypes.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null) return NotFound();

            var dto = _mapper.ToDto(entity);
            return View(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania PartType do usunięcia o Id={PartTypeId}", id);
            return StatusCode(500, "Wystąpił błąd podczas pobierania danych.");
        }
    }

    // POST: PartType/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _context.PartTypes.FindAsync(id);
        if (entity == null)
        {
            _logger.LogWarning("Próba usunięcia nieistniejącego PartType o Id={PartTypeId}", id);
            return RedirectToAction(nameof(Index));
        }

        _context.PartTypes.Remove(entity);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}