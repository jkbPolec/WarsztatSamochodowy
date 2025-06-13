namespace WarsztatSamochodowyApp.DTO;

public class UsedPartDto
{
    public int PartId { get; set; }
    public int Quantity { get; set; }

    public PartDto Part { get; set; } = new();
}