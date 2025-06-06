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
    private readonly PartMapper _mapper;

    public PartsController(ApplicationDbContext context, PartMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: Parts
    public async Task<IActionResult> Index()
    {
        var parts = await _context.Parts.Include(p => p.PartType).ToListAsync();
        var partDtos = parts.Select(p => _mapper.ToDto(p)).ToList();
        return View(partDtos);
    }

    // GET: Parts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var part = await _context.Parts
            .Include(p => p.PartType)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (part == null) return NotFound();

        return View(part);
    }

    // GET: Parts/Create
    public IActionResult Create()
    {
        ViewData["PartTypeId"] = new SelectList(_context.PartTypes, "Id", "Name");
        return View(new PartDto());
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

        foreach (var error in ModelState.Values.SelectMany(v => v.Errors)) Console.WriteLine(error.ErrorMessage);
        ViewData["PartTypeId"] = new SelectList(_context.PartTypes, "Id", "Name", dto.PartTypeId);
        return View(dto);
    }

    // GET: Parts/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var part = await _context.Parts.FindAsync(id);
        if (part == null) return NotFound();
        var dto = _mapper.ToDto(part);
        ViewData["PartTypeId"] = new SelectList(_context.PartTypes, "Id", "Name", dto.PartTypeId);
        return View(dto);
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
            if (part == null) return NotFound();

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

        var part = await _context.Parts
            .Include(p => p.PartType)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (part == null) return NotFound();

        return View(part);
    }

    // POST: Parts/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var part = await _context.Parts.FindAsync(id);
        if (part != null) _context.Parts.Remove(part);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PartExists(int id)
    {
        return _context.Parts.Any(e => e.Id == id);
    }
}