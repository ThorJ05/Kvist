namespace API.DTOs;

// Get's sent back to the client. Never contains password or hash.
public record UserDto(string Id, string Username, List<string> Roles);