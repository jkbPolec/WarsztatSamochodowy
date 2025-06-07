namespace WarsztatSamochodowyApp.Models;

public class ServiceTask
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public int ServiceOrderId { get; set; }
    public ServiceOrder ServiceOrder { get; set; } = null!;
}