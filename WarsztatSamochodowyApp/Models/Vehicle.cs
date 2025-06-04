namespace WarsztatSamochodowyApp.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Vin { get; set; }
    public string RegistrationNumber { get; set; }
    public string ImageUrl { get; set; }
    
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    
    
}