using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

// All fields are optinal: only the field that have been changed.
public class UpdateUserDto
{
    [MinLength(3), MaxLength(50)]
    public string? Username { get; set; }

    [MinLength(8)]
    public string? Password { get; set; }

    public string? Role { get; set; }
}