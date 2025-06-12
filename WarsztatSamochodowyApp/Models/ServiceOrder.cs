using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WarsztatSamochodowyApp.Models;

public class ServiceOrder
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public ServiceOrderStatus Status { get; set; }
    public DateTime? FinishedDate { get; set; }


    public int VehicleId { get; set; }

    [ValidateNever] public Vehicle Vehicle { get; set; } = null!;

    // Tworzy relacje wiele do wielu z ServiceTask
    public ICollection<ServiceTask> ServiceTasks { get; set; } = new List<ServiceTask>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();


    public string? MechanicId { get; set; } // FK do AppUser (nullable, jak nieprzypisane)

    [ValidateNever] public string? MechanicName { get; set; } // do wyświetlania, opcjonalne
}