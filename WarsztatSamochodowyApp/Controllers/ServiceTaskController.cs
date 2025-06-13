using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Mappers;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers;

public class ServiceTaskController : Controller
{
    private readonly ApplicationDbContext _context;

    private readonly ILogger<ServiceTaskController> _logger;

    // ZMIANA: Wstrzykujemy nowy, dedykowany mapper
    private readonly ServiceTaskMapper _mapper;

    public ServiceTaskController(ApplicationDbContext context, ILogger<ServiceTaskController> logger,
        ServiceTaskMapper mapper) // <-- ZMIANA
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    // Metody Index, Details, Delete (GET), DeleteConfirmed - zostają takie same,
    // ale będą teraz używać nowego, poprawnego mappera.

    public async Task<IActionResult> Index()
    {
        var tasks = await _context.ServiceTasks
            .Include(t => t.UsedParts).ThenInclude(up => up.Part)
            .ToListAsync();
        return View(_mapper.ToDtoList(tasks));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var serviceTask = await _context.ServiceTasks
            .Include(t => t.UsedParts).ThenInclude(up => up.Part)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (serviceTask == null) return NotFound();
        return View(_mapper.ToDto(serviceTask));
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.AllParts = await _context.Parts.ToListAsync();
        return View(new ServiceTaskDto());
    }

    // ZMIANA: Logika tworzenia pozostaje w kontrolerze
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceTaskDto dto, List<int> partIds, List<int> quantities)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AllParts = await _context.Parts.ToListAsync();
            return View(dto);
        }

        try
        {
            var entity = new ServiceTask
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price
            };

            if (partIds != null && quantities != null && partIds.Count == quantities.Count)
                entity.UsedParts = partIds.Zip(quantities, (id, q) => new { id, q })
                    .Where(x => x.id > 0 && x.q > 0)
                    .Select(x => new UsedPart { PartId = x.id, Quantity = x.q })
                    .ToList();

            _context.ServiceTasks.Add(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia zadania serwisowego.");
            ModelState.AddModelError("", "Wystąpił błąd podczas zapisu.");
            ViewBag.AllParts = await _context.Parts.ToListAsync();
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var serviceTask = await _context.ServiceTasks
            .Include(t => t.UsedParts).ThenInclude(up => up.Part)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (serviceTask == null) return NotFound();

        ViewBag.AllParts = await _context.Parts.ToListAsync();
        return View(_mapper.ToDto(serviceTask));
    }

    // ZMIANA: Logika aktualizacji pozostaje w kontrolerze
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceTaskDto dto, List<int> partIds, List<int> quantities)
    {
        if (id != dto.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.AllParts = await _context.Parts.ToListAsync();
            return View(dto);
        }

        try
        {
            var existingTask = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (existingTask == null) return NotFound();

            // Używamy mappera tylko do prostych pól
            _mapper.UpdateEntityFromDto(dto, existingTask);

            // Logika aktualizacji relacji zostaje w kontrolerze
            existingTask.UsedParts.Clear();
            if (partIds != null && quantities != null && partIds.Count == quantities.Count)
            {
                var newParts = partIds.Zip(quantities, (pId, q) => new { pId, q })
                    .Where(x => x.pId > 0 && x.q > 0)
                    .Select(x => new UsedPart { PartId = x.pId, Quantity = x.q });

                foreach (var part in newParts) existingTask.UsedParts.Add(part);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas edycji zadania serwisowego o ID {TaskId}", id);
            ModelState.AddModelError("", "Wystąpił błąd podczas zapisu zmian.");
            ViewBag.AllParts = await _context.Parts.ToListAsync();
            return View(dto);
        }
    }

    // Delete i DeleteConfirmed bez zmian.
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var serviceTask = await _context.ServiceTasks.FirstOrDefaultAsync(m => m.Id == id);
        if (serviceTask == null) return NotFound();
        return View(_mapper.ToDto(serviceTask));
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var serviceTask = await _context.ServiceTasks.FindAsync(id);
        if (serviceTask != null)
        {
            _context.ServiceTasks.Remove(serviceTask);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}