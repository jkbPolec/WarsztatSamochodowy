namespace WarsztatSamochodowyApp.DTO;

public class ServiceTaskDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; } // Cena samej usługi (TotalCost będzie obliczany)

    public List<UsedPartDto> UsedParts { get; set; } = new();

    public decimal TotalCost
    {
        get
        {
            var partsCost = UsedParts?.Sum(up => up.Quantity * (up.Part?.Price ?? 0)) ?? 0;
            return Price + partsCost;
        }
    }

    public List<UsedPartInputDto> PartsInput { get; set; } = new();
}

// Nowa klasa DTO, która będzie reprezentować JEDEN wiersz w formularzu części
public class UsedPartInputDto
{
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public PartDto Part { get; set; }
}