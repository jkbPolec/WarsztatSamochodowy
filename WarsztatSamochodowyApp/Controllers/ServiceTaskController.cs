using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Mappers;

namespace WarsztatSamochodowyApp.Controllers;

public class ServiceTaskController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ServiceTaskController> _logger;
    private readonly ServicesMapper _mapper;

    public ServiceTaskController(ApplicationDbContext context, ILogger<ServiceTaskController> logger,
        ServicesMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var tasks = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .ThenInclude(up => up.Part)
                .ToListAsync();

            var dtos = tasks.Select(_mapper.ToDto).ToList();
            return View(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania listy zadań serwisowych.");
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var serviceTask = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (serviceTask == null) return NotFound();

            var dto = _mapper.ToDto(serviceTask);
            return View(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania szczegółów zadania serwisowego o ID {TaskId}", id);
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            ViewBag.AllParts = await _context.Parts.ToListAsync();
            return View(new ServiceTaskDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przygotowywania formularza tworzenia zadania serwisowego.");
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

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
            if (partIds != null && quantities != null && partIds.Count == quantities.Count)
                dto.UsedPartIds = partIds.Zip(quantities, (id, q) => new { id, q })
                    .Where(x => x.q > 0)
                    .Select(x => x.id)
                    .ToList();

            var entity = _mapper.ToEntity(dto);
            _context.ServiceTasks.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia zadania serwisowego.");
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var serviceTask = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (serviceTask == null) return NotFound();

            ViewBag.AllParts = await _context.Parts.ToListAsync();

            var dto = _mapper.ToDto(serviceTask);
            return View(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przygotowywania edycji zadania o ID {TaskId}", id);
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

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

            if (partIds != null && quantities != null && partIds.Count == quantities.Count)
                dto.UsedPartIds = partIds.Zip(quantities, (pid, q) => new { pid, q })
                    .Where(x => x.q > 0)
                    .Select(x => x.pid)
                    .ToList();

            _mapper.UpdateEntity(dto, existingTask);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas edycji zadania serwisowego o ID {TaskId}", id);
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        try
        {
            var serviceTask = await _context.ServiceTasks
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceTask == null) return NotFound();

            var dto = _mapper.ToDto(serviceTask);
            return View(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas ładowania zadania do usunięcia o ID {TaskId}", id);
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var serviceTask = await _context.ServiceTasks.FindAsync(id);
            if (serviceTask != null)
            {
                _context.ServiceTasks.Remove(serviceTask);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania zadania serwisowego o ID {TaskId}", id);
            return StatusCode(500, "Wystąpił błąd.");
        }
    }

    private bool ServiceTaskExists(int id)
    {
        return _context.ServiceTasks.Any(e => e.Id == id);
    }
}