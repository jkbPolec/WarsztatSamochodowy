namespace WarsztatSamochodowyApp.Models;

public class UsedPart
{
    public int Id { get; set; }
    public int ServiceTaskId { get; set; }
    public ServiceTask ServiceTask { get; set; } = null!;
    
    public int PartId { get; set; }
    public Part Part { get; set; } = null!;
    
    public int Quantity { get; set; }
}