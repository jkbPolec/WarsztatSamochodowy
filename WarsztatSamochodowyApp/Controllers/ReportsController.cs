using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Services; // Zastąp poprawną przestrzenią nazw
using WarsztatSamochodowyApp.Services.Reports;
using WarsztatSamochodowyApp.Services.Pdf;

namespace WarsztatSamochodowyApp.Controllers;

public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly ApplicationDbContext _context; // Do pobrania listy klientów i pojazdów
    private readonly MonthlyRepairPdfExporter _pdfExporter;
    
    public ReportsController(IReportService reportService, ApplicationDbContext context, MonthlyRepairPdfExporter pdfExporter)
    {
        _reportService = reportService;
        _context = context;
        _pdfExporter = pdfExporter;
    }
    
    public async Task<byte[]> GenerateMonthlySummaryPdfAsync(int year, int month)
    {
        var data = await _reportService.GenerateMonthlySummaryAsync(year, month);


        if (!data.Any())
            return null!;

        return _pdfExporter.Generate(year, month, data);
    }
    // GET: /Reports/Index
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    // GET: /Reports/ClientRepairs
    [HttpGet]
    public async Task<IActionResult> ClientRepairs()
    {
        // Przygotuj dane dla dropdowna z klientami
        // Przekazanie wygenerowanego raportu do tego samego widoku
        ViewBag.Clients = _context.Clients
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"[{c.Id}] {c.LastName} {c.FirstName}"
            })
            .ToList();
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
        ViewBag.Clients = _context.Clients
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"[{c.Id}] {c.LastName} {c.FirstName}"
            })
            .ToList();
        ViewBag.ReportData = report;


        return View("ClientRepairs");
    }
    
    // GET: /Reports/MonthlySummary
    [HttpGet]
    public IActionResult MonthlySummary()
    {
        return View();
    }
    
    // POST: /Reports/MonthlySummary
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateMonthlySummaryPdf(int year, int month)
    {
        var data = await _reportService.GenerateMonthlySummaryAsync(year, month);

        if (!data.Any())
        {
            TempData["Message"] = "Brak danych do raportu.";
            return RedirectToAction("MonthlySummary");
        }

        // Użyj klasy MonthlyRepairPdfExporter do wygenerowania PDF-a
        var pdfBytes = _pdfExporter.Generate(year, month, data);

        return File(pdfBytes, "application/pdf", $"Raport_{month:00}_{year}.pdf");
    }

}