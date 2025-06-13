using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context; // Wstrzyknij DbContext
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Pobieramy statystyki i przekazujemy je przez ViewBag
        ViewBag.ActiveOrdersCount = await _context.ServiceOrders
            .CountAsync(o => o.Status != ServiceOrderStatus.Zakonczone && o.Status != ServiceOrderStatus.Anulowane);

        ViewBag.TotalVehiclesCount = await _context.Vehicles.CountAsync();

        ViewBag.ClientsCount = await _context.Clients.CountAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        _logger.LogError("Wywołano akcję Error. RequestId: {RequestId}",
            Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}