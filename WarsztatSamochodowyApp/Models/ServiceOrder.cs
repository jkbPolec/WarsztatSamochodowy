namespace WarsztatSamochodowyApp.Models;

public class ServiceOrder
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    
    //public int WorkerId { get; set; }
    //public Worker Worker { get; set; } = null!;
}