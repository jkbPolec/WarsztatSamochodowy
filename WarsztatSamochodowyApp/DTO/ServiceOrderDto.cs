namespace WarsztatSamochodowyApp.DTO;

public class ServiceOrderDto
{
    public int Id { get; set; }
    public string Description { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public int MechanicId { get; set; }
    public string? MechanicName { get; set; }

    public string? Comments { get; set; }

    public VehicleDto? Vehicle { get; set; }

    public List<ServiceTaskDto> ServiceTasks { get; set; } = new();
}

public class ServiceTaskDto
{
    public int Id { get; set; }
    public string TaskDescription { get; set; } = null!;
    public bool IsCompleted { get; set; }
}