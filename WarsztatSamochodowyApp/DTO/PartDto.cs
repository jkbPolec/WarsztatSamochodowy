// DTO/PartDto.cs

using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.DTO;

public class PartDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int PartTypeId { get; set; }
    public PartType? PartType { get; set; }
}