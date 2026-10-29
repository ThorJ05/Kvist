using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateUserDto
{
    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; } = "";

    [Required, MinLength(8)]
    public string Password { get; set; } = "";

    [Required]
    public string Role { get; set; } = "User";
}