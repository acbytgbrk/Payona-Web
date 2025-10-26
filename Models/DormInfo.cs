namespace Payona.API.Models;

public class DormInfo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Dorm { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public User User { get; set; } = null!;
}