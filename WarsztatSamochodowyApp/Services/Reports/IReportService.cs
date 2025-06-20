﻿using WarsztatSamochodowyApp.DTO;

namespace WarsztatSamochodowyApp.Services.Reports;

public interface IReportService {
    Task<ClientRepairReportDto?> GenerateClientRepairsReportAsync(int clientId, int vehicleId, int? year, int? month);
    Task<List<MonthlyRepairSummaryDto>> GenerateMonthlySummaryAsync(int year, int month);
    Task<List<CurrentServiceOrderReportItemDto>> GenerateCurrentServiceOrdersReportAsync();
}