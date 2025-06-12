namespace WarsztatSamochodowyApp.DTO;

public class ServiceTaskDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public List<UsedPartDto> UsedParts { get; set; } = new();

    public List<int> UsedPartIds { get; set; } = new();

    public decimal PartsCost => UsedParts.Sum(up => up.Part.Price * up.Quantity);
    public decimal TotalCost => Price + PartsCost;
}