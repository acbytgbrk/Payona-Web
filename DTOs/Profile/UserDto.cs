namespace Payona.API.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public string? City { get; set; }
    public string? Dorm { get; set; }
}