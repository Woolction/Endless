namespace Backend.API.Dtos;

public record class AuthRequestDto(string? Name, string Email, string Password);