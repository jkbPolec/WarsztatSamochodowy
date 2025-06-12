using WarsztatSamochodowyApp.DTO;

namespace WarsztatSamochodowyApp.Services.Reports;

public interface IReportService {
    Task<ClientRepairReportViewModel?> GenerateClientRepairsReportAsync(int clientId, int vehicleId, int? year, int? month);
}