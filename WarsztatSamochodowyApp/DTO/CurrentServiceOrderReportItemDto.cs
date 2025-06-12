namespace WarsztatSamochodowyApp.DTO;

public class CurrentServiceOrderReportItemDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string MechanicFullName { get; set; } = string.Empty;
    public string VehicleIdentifier { get; set; } = string.Empty; // np. "VIN - Rejestracja"
    public string ClientFullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
