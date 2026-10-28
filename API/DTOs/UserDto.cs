namespace API.DTOs;

// Det der sendes tilbage til klienten. Indeholder aldrig password eller hash.
public record UserDto(string Id, string Username, List<string> Roles);