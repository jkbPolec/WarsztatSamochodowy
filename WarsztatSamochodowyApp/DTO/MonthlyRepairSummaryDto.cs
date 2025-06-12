namespace WarsztatSamochodowyApp.DTO;

public class MonthlyRepairSummaryDto {
    public string ClientFullName { get; set; } = "";
    public string VehicleIdentifier { get; set; } = "";
    public int OrdersCount { get; set; }
    public decimal TotalCost { get; set; }
}