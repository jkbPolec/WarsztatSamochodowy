using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data; // Zastąp poprawną przestrzenią nazw
using WarsztatSamochodowyApp.Services.Reports;

namespace WarsztatSamochodowyApp.Controllers;

public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly ApplicationDbContext _context; // Do pobrania listy klientów i pojazdów

    public ReportsController(IReportService reportService, ApplicationDbContext context)
    {
        _reportService = reportService;
        _context = context;
    }

    // GET: /Reports/ClientRepairs
    [HttpGet]
    public async Task<IActionResult> ClientRepairs()
    {
        // Przygotuj dane dla dropdowna z klientami
        ViewBag.Clients = new SelectList(await _context.Clients.ToListAsync(), "Id", "LastName");
        return View();
    }
    
    // Akcja do dynamicznego pobierania pojazdów klienta (opcjonalnie, dla lepszego UX)
    [HttpGet]
    public async Task<JsonResult> GetVehiclesForClient(int clientId)
    {
        var vehicles = await _context.Vehicles
            .Where(v => v.ClientId == clientId)
            .Select(v => new { id = v.Id, text = $"{v.RegistrationNumber} ({v.Vin})" })
            .ToListAsync();
        return Json(vehicles);
    }


    // POST: /Reports/GenerateClientRepairsReport
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateClientRepairsReport(int clientId, int vehicleId, int? year, int? month)
    {
        if (clientId == 0 || vehicleId == 0)
        {
            ModelState.AddModelError("", "Proszę wybrać klienta i pojazd.");
            ViewBag.Clients = new SelectList(await _context.Clients.ToListAsync(), "Id", "LastName");
            return View("ClientRepairs");
        }

        var report = await _reportService.GenerateClientRepairsReportAsync(clientId, vehicleId, year, month);

        if (report == null)
        {
            return NotFound("Nie znaleziono pojazdu dla podanego klienta.");
        }
        
        // Przekazanie wygenerowanego raportu do tego samego widoku
        ViewBag.Clients = new SelectList(await _context.Clients.ToListAsync(), "Id", "LastName");
        ViewBag.ReportData = report;


        return View("ClientRepairs");
    }
}