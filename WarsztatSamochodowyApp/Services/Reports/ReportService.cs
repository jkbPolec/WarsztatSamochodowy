using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.DTO;

namespace WarsztatSamochodowyApp.Services.Reports;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context; // Zastąp ApplicationDbContext nazwą Twojego DbContext

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClientRepairReportViewModel?> GenerateClientRepairsReportAsync(int clientId, int vehicleId, int? year, int? month)
    {
        // 1. Znajdź klienta i pojazd, aby upewnić się, że istnieją i są powiązane
        var vehicle = await _context.Vehicles
            .Include(v => v.Client)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == vehicleId && v.ClientId == clientId);

        if (vehicle == null)
        {
            return null; // Nie znaleziono pojazdu lub nie należy do tego klienta
        }

        // 2. Budowanie zapytania o zlecenia serwisowe
        var query = _context.ServiceOrders
            .AsNoTracking()
            .Where(so => so.VehicleId == vehicleId);

        // 3. Opcjonalne filtrowanie po dacie
        string dateRange = "Wszystkie naprawy";
        if (year.HasValue && month.HasValue)
        {
            query = query.Where(so => so.OrderDate.Year == year.Value && so.OrderDate.Month == month.Value);
            dateRange = $"Naprawy z: {month:00}/{year}";
        }

        // 4. Pobranie i przetworzenie danych
        var repairs = await query
            .Include(so => so.ServiceTasks)
                .ThenInclude(st => st.UsedParts)
                    .ThenInclude(up => up.Part)
            .Select(so => new ClientRepairReportViewModel.RepairItem
            {
                ServiceOrderId = so.Id,
                OrderDate = so.OrderDate,
                Status = so.Status.ToString(),
                TasksPerformed = so.ServiceTasks.Select(st => st.Name).ToList(),
                // Suma kosztów robocizny (ceny wszystkich zadań w zleceniu)
                LaborCost = so.ServiceTasks.Sum(st => st.Price),
                // Suma kosztów części (ilość * cena dla każdej części we wszystkich zadaniach)
                PartsCost = so.ServiceTasks.SelectMany(st => st.UsedParts).Sum(up => up.Quantity * up.Part.Price)
            })
            .OrderByDescending(r => r.OrderDate)
            .ToListAsync();

        // 5. Zbudowanie finalnego ViewModelu
        var report = new ClientRepairReportViewModel
        {
            ClientFullName = $"{vehicle.Client.FirstName} {vehicle.Client.LastName}",
            VehicleIdentifier = $"{vehicle.Vin} - {vehicle.RegistrationNumber}",
            ReportDateRange = dateRange,
            Repairs = repairs,
            GrandTotal = repairs.Sum(r => r.TotalCost)
        };

        return report;
    }
}