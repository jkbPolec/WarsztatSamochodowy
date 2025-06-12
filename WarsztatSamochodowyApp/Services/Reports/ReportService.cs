using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Services.Reports;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context; // Zastąp ApplicationDbContext nazwą Twojego DbContext

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClientRepairReportDto?> GenerateClientRepairsReportAsync(int clientId, int vehicleId, int? year, int? month)
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
            .Select(so => new ClientRepairReportDto.RepairItem
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
        var report = new ClientRepairReportDto
        {
            ClientFullName = $"{vehicle.Client.FirstName} {vehicle.Client.LastName}",
            VehicleIdentifier = $"{vehicle.Vin} - {vehicle.RegistrationNumber}",
            ReportDateRange = dateRange,
            Repairs = repairs,
            GrandTotal = repairs.Sum(r => r.TotalCost)
        };

        return report;
    }
    
    public async Task<List<MonthlyRepairSummaryDto>> GenerateMonthlySummaryAsync(int year, int month)
    {
        return await _context.ServiceOrders
            .AsNoTracking()
            .Where(so => so.OrderDate.Year == year && so.OrderDate.Month == month)
            .Include(so => so.Vehicle).ThenInclude(v => v.Client)
            .Include(so => so.ServiceTasks).ThenInclude(st => st.UsedParts).ThenInclude(up => up.Part)
            .GroupBy(so => new
            {
                ClientFullName = so.Vehicle.Client.FirstName + " " + so.Vehicle.Client.LastName,
                VehicleIdentifier = so.Vehicle.Vin + " - " + so.Vehicle.RegistrationNumber
            })
            .Select(g => new MonthlyRepairSummaryDto
            {
                ClientFullName = g.Key.ClientFullName,
                VehicleIdentifier = g.Key.VehicleIdentifier,
                OrdersCount = g.Count(),
                TotalCost = g.Sum(so =>
                    so.ServiceTasks.Sum(st => st.Price) +
                    so.ServiceTasks.SelectMany(st => st.UsedParts).Sum(up => up.Quantity * up.Part.Price))
            })
            .OrderBy(r => r.ClientFullName)
            .ToListAsync();
    }
    
    public async Task<List<CurrentServiceOrderReportItemDto>> GenerateCurrentServiceOrdersReportAsync()
    {
        // Słownik według username lub email, zależnie czym jest MechanicId
        var mechanics = await _context.Workers
            .AsNoTracking()
            .ToDictionaryAsync(w => w.username, w => w.firstName + " " + w.lastName); // lub w.email

        var orders = await _context.ServiceOrders
            .AsNoTracking()
            .Where(so => so.Status == ServiceOrderStatus.Nowe || so.Status == ServiceOrderStatus.WTrakcie)
            .Include(so => so.Vehicle).ThenInclude(v => v.Client)
            .Select(so => new
            {
                so.Id,
                so.OrderDate,
                so.MechanicId,
                so.Status,
                VehicleVin = so.Vehicle.Vin,
                VehicleReg = so.Vehicle.RegistrationNumber,
                ClientName = so.Vehicle.Client.FirstName + " " + so.Vehicle.Client.LastName
            })
            .ToListAsync();

        var result = orders.Select(so => new CurrentServiceOrderReportItemDto
        {
            OrderId = so.Id,
            OrderDate = so.OrderDate,
            MechanicFullName = mechanics.TryGetValue(so.MechanicId ?? "", out var name) ? name : "Nieznany",
            VehicleIdentifier = $"{so.VehicleVin} - {so.VehicleReg}",
            ClientFullName = so.ClientName,
            Status = so.Status.ToString()
        }).OrderByDescending(r => r.OrderDate).ToList();

        return result;
    }




}