namespace WarsztatSamochodowyApp.Models;

public class Part
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    public int PartTypeId { get; set; }
    public PartType PartType { get; set; } = null!;
}