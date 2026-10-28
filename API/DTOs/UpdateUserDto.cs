using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

// Alle felter er valgfrie: kun de felter, der er sat, bliver ændret.
public class UpdateUserDto
{
    [MinLength(3), MaxLength(50)]
    public string? Username { get; set; }

    [MinLength(8)]
    public string? Password { get; set; }

    public string? Role { get; set; }
}