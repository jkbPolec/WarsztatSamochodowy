namespace WarsztatSamochodowyApp.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ServiceOrderId { get; set; }
    public ServiceOrder ServiceOrder { get; set; } = null!;

    public int WorkerId { get; set; }
    public Worker Worker { get; set; } = null!;
}