namespace WarsztatSamochodowyApp.DTO;

public class VehicleDto
{
    public int Id { get; set; }
    public string Vin { get; set; }
    public string RegistrationNumber { get; set; }
    public string? ImageUrl { get; set; }
    public string ClientFullName { get; set; }
    public int ClientId { get; set; } // do create/edit
}