using System.ComponentModel.DataAnnotations;

namespace Payona.API.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "E-posta gerekli")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre gerekli")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı")]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Surname { get; set; } = string.Empty;
}