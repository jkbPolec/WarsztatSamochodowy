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

public class VehicleDto
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = null!;

    public string Model { get; set; } = null!;
    // dodaj co tam jeszcze chcesz z pojazdu
}

public class ServiceTaskDto
{
    public int Id { get; set; }
    public string TaskDescription { get; set; } = null!;
    public bool IsCompleted { get; set; }
}