namespace WarsztatSamochodowyApp.DTO;

public class ClientRepairReportViewModel
{
    public string ClientFullName { get; set; }
    public string VehicleIdentifier { get; set; } // np. "VIN - Rejestracja"
    public string ReportDateRange { get; set; }
    public List<RepairItem> Repairs { get; set; } = new();
    public decimal GrandTotal { get; set; }

    public class RepairItem
    {
        public int ServiceOrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public List<string> TasksPerformed { get; set; } = new();
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal TotalCost => LaborCost + PartsCost;
    }   
}