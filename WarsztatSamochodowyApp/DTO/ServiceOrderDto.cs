using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.DTO;

public class ServiceOrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public ServiceOrderStatus Status { get; set; }
    public DateTime? FinishedDate { get; set; }

    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }

    public List<int> SelectedTaskIds { get; set; } = new();

    public List<ServiceTaskDto> ServiceTasks { get; set; } = new();
    public List<CommentDto> Comments { get; set; } = new();

    public string? MechanicId { get; set; }
    public string? MechanicName { get; set; }
}