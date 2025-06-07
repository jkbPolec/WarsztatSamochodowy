namespace WarsztatSamochodowyApp.Models;

public class ServiceTask
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }

    // Tworzy relacje wiele do wielu z ServiceOrder
    public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
}