namespace Payona.API.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;

    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordExpires { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // User → DormInfo (1:1)
    public DormInfo? DormInfo { get; set; }

    // Navigation Properties
    public ICollection<Fingerprint> Fingerprints { get; set; } = new List<Fingerprint>();
    public ICollection<MealRequest> MealRequests { get; set; } = new List<MealRequest>();
    public ICollection<Match> GivenMatches { get; set; } = new List<Match>();
    public ICollection<Match> ReceivedMatches { get; set; } = new List<Match>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
}