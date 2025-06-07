using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WarsztatSamochodowyApp.Models;

public class ServiceOrder
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public ServiceOrderStatus Status { get; set; }
    

    public int VehicleId { get; set; }
    [ValidateNever]
    public Vehicle Vehicle { get; set; } = null!;
    
    //public int WorkerId { get; set; }
    //public Worker Worker { get; set; } = null!;
}