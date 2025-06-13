using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Mappers;
using WarsztatSamochodowyApp.Models;

// Upewnij się, że masz using do mapperów

// Upewnij się, że masz using do DTO

namespace WarsztatSamochodowyApp.Controllers;

public class ServiceOrderController : Controller
{
    private readonly ApplicationDbContext _context;

    private readonly ILogger<ServiceOrderController> _logger;
    private readonly ServicesMapper _mapper;

    private readonly UserManager<AppUser> _userManager;
    private readonly VehicleMapper _vehicleMapper;

    public ServiceOrderController(ApplicationDbContext context, ILogger<ServiceOrderController> logger,
        UserManager<AppUser> userManager, ServicesMapper mapper, VehicleMapper vehicleMapper)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
        _mapper = mapper;
        _vehicleMapper = vehicleMapper;
    }

    // --- Metody GET (Index, Details, Create, Edit, Delete) - wydają się w porządku, zostawiam bez większych zmian ---
    // (Poniżej wklejam je dla kompletności, ale kluczowe zmiany są w POST)

    public async Task<IActionResult> Index(ServiceOrderStatus? statusFilter)
    {
        try
        {
            var query = _context.ServiceOrders.Include(v => v.Vehicle).AsQueryable();

            if (statusFilter.HasValue)
                query = query.Where(o => o.Status == statusFilter.Value);

            var serviceOrders = await query.ToListAsync();
            var mechanics = await _userManager.GetUsersInRoleAsync("Mechanik");
            var mechanicsDict = mechanics.ToDictionary(m => m.Id, m => m.Email);

            var dtoList = new List<ServiceOrderDto>();
            foreach (var order in serviceOrders)
            {
                if (order.MechanicId != null && mechanicsDict.ContainsKey(order.MechanicId))
                    order.MechanicName = mechanicsDict[order.MechanicId];
                else
                    order.MechanicName = "Brak";

                dtoList.Add(_mapper.ToDto(order));
            }

            ViewData["StatusFilter"] =
                new SelectList(
                    Enum.GetValues(typeof(ServiceOrderStatus)).Cast<ServiceOrderStatus>()
                        .Select(s => new { Value = s, Text = s.ToString() }), "Value", "Text", statusFilter);
            return View(dtoList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania listy zleceń serwisowych.");
            return StatusCode(500, "Wystąpił błąd podczas pobierania zleceń.");
        }
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        // ... Twoja obecna logika jest OK
        var serviceOrder = await _context.ServiceOrders
            .Include(o => o.Vehicle)
            .Include(o => o.ServiceTasks)
            .ThenInclude(st => st.UsedParts)
            .ThenInclude(up => up.Part)
            .Include(o => o.Comments.OrderByDescending(c => c.CreatedAt))
            .FirstOrDefaultAsync(m => m.Id == id);
        if (serviceOrder == null) return NotFound();
        if (!string.IsNullOrEmpty(serviceOrder.MechanicId))
        {
            var mechanik = await _userManager.FindByIdAsync(serviceOrder.MechanicId);
            serviceOrder.MechanicName = mechanik?.UserName;
        }

        var dto = _mapper.ToDto(serviceOrder);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, string content)
    {
        // Twoja obecna logika jest OK
        if (string.IsNullOrWhiteSpace(content)) return RedirectToAction("Details", new { id });
        var comment = new Comment { ServiceOrderId = id, Content = content, Author = User.Identity?.Name ?? "Anonim" };
        _context.ServiceOrderComments.Add(comment);
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    // GET: ServiceOrder/Create
    public async Task<IActionResult> Create()
    {
        await PrepareViewDataForCreateAndEdit(new ServiceOrderDto());
        return View(new ServiceOrderDto());
    }

    // =================================================================
    // POPRAWIONA METODA CREATE (POST)
    // =================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceOrderDto dto) // <-- KLUCZOWA ZMIANA: tylko DTO jako parametr
    {
        try
        {
            // `SelectedTaskIds` jest teraz właściwością `dto`
            if (ModelState.IsValid)
            {
                // Tworzymy nową encję i mapujemy do niej dane z DTO
                var newOrder = new ServiceOrder
                {
                    OrderDate = DateTime.Now,
                    Status = ServiceOrderStatus.Nowe,
                    VehicleId = dto.VehicleId,
                    MechanicId = dto.MechanicId
                };

                // Przypisujemy wybrane zadania
                if (dto.SelectedTaskIds != null && dto.SelectedTaskIds.Any())
                {
                    var selectedTasks = await _context.ServiceTasks
                        .Where(t => dto.SelectedTaskIds.Contains(t.Id))
                        .ToListAsync();
                    newOrder.ServiceTasks = selectedTasks;
                }

                _context.Add(newOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia zlecenia serwisowego.");
            ModelState.AddModelError("", "Wystąpił nieoczekiwany błąd podczas zapisu zlecenia. Spróbuj ponownie.");
        }

        // Jeśli model jest niepoprawny lub wystąpił błąd, przygotuj widok ponownie
        await PrepareViewDataForCreateAndEdit(dto);
        return View(dto);
    }

    // GET: ServiceOrder/Edit/{id}
    [Authorize(Policy = "OnlyAssignedMechanic")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var serviceOrder = await _context.ServiceOrders
            .Include(o => o.ServiceTasks)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (serviceOrder == null) return NotFound();

        var dto = _mapper.ToDto(serviceOrder);
        await PrepareViewDataForCreateAndEdit(dto);

        return View(dto);
    }

    // =================================================================
    // POPRAWIONA METODA EDIT (POST)
    // =================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceOrderDto dto) // <-- KLUCZOWA ZMIANA: tylko DTO jako parametr
    {
        if (id != dto.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            await PrepareViewDataForCreateAndEdit(dto);
            return View(dto);
        }

        try
        {
            // Pobieramy istniejącą, śledzoną encję z bazy
            var existingOrder = await _context.ServiceOrders
                .Include(o => o.ServiceTasks)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (existingOrder == null) return NotFound();

            // Aktualizujemy właściwości istniejącej encji na podstawie DTO
            existingOrder.Status = dto.Status;
            existingOrder.VehicleId = dto.VehicleId;
            existingOrder.MechanicId = dto.MechanicId;

            // Logika daty zakończenia
            if (dto.Status == ServiceOrderStatus.Zakonczone && existingOrder.FinishedDate == null)
                existingOrder.FinishedDate = DateTime.Now;
            else if (dto.Status != ServiceOrderStatus.Zakonczone)
                existingOrder.FinishedDate = null;

            // Aktualizujemy listę zadań
            existingOrder.ServiceTasks.Clear(); // Czyścimy stare powiązania
            if (dto.SelectedTaskIds != null && dto.SelectedTaskIds.Any())
            {
                var selectedTasks = await _context.ServiceTasks
                    .Where(t => dto.SelectedTaskIds.Contains(t.Id))
                    .ToListAsync();
                foreach (var task in selectedTasks) existingOrder.ServiceTasks.Add(task);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Błąd współbieżności przy edycji zlecenia o ID {OrderId}", id);
            ModelState.AddModelError("",
                "Dane zostały w międzyczasie zmienione przez innego użytkownika. Odśwież stronę i spróbuj ponownie.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas edycji zlecenia o ID {OrderId}", id);
            ModelState.AddModelError("", "Wystąpił nieoczekiwany błąd podczas zapisu zmian.");
        }

        await PrepareViewDataForCreateAndEdit(dto);
        return View(dto);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        // ... Twoja obecna logika jest OK
        if (id == null) return NotFound();
        var serviceOrder = await _context.ServiceOrders.Include(v => v.Vehicle).FirstOrDefaultAsync(m => m.Id == id);
        if (serviceOrder == null) return NotFound();
        var dto = _mapper.ToDto(serviceOrder);
        if (!string.IsNullOrEmpty(dto.MechanicId))
        {
            var mechanik = await _userManager.FindByIdAsync(dto.MechanicId);
            dto.MechanicName = mechanik?.UserName;
        }

        return View(dto);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var serviceOrder = await _context.ServiceOrders.FindAsync(id);
        if (serviceOrder != null)
        {
            _context.ServiceOrders.Remove(serviceOrder);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }


    private bool ServiceOrderExists(int id)
    {
        return _context.ServiceOrders.Any(e => e.Id == id);
    }

    // Prywatna metoda pomocnicza do wypełniania ViewBag/ViewData, aby uniknąć powtórzeń
    private async Task PrepareViewDataForCreateAndEdit(ServiceOrderDto dto)
    {
        var vehicles = await _context.Vehicles.ToListAsync();
        // ZMIANA 1: Pracujemy bezpośrednio na liście encji 'vehicles'.
        // Konstruktor SelectList potrafi sam pobrać właściwości "Id" i "RegistrationNumber" z obiektów Vehicle.
        // Nie ma potrzeby tworzyć listy DTO tylko dla tego celu.
        ViewData["VehicleId"] = new SelectList(vehicles, "Id", "RegistrationNumber", dto.VehicleId);

        var mechanics = await _userManager.GetUsersInRoleAsync("Mechanik");
        ViewBag.MechanicList = new SelectList(mechanics, "Id", "Email", dto.MechanicId);

        var allTasks = await _context.ServiceTasks.ToListAsync();
        // ZMIANA 2: Używamy LINQ .Select() do transformacji listy encji 'allTasks' na listę SelectListItem.
        // Dla każdego zadania 't' w liście 'allTasks' tworzymy nowy SelectListItem.
        // To eliminuje potrzebę posiadania metody ToDtoList.
        ViewBag.ServiceTasks = allTasks.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            // Użyłem 'TotalCost' jak w oryginalnym kodzie, jeśli w encji pole nazywa się inaczej (np. Price), zmień je tutaj.
            Text = $"{t.Name} - {t.TotalCost:C}",
            Selected = dto.SelectedTaskIds.Contains(t.Id)
        }).ToList();
    }
}