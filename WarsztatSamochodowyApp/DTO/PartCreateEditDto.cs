// DTO/PartCreateEditDto.cs

namespace WarsztatSamochodowyApp.DTO;

public class PartCreateEditDto
{
    public int Id { get; set; } // opcjonalnie przy create może być ignorowany
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int PartTypeId { get; set; }
}