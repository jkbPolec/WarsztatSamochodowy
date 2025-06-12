namespace WarsztatSamochodowyApp.DTO;

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public string Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public int ServiceOrderId { get; set; }
}