using Backend.API.Data.Components;

namespace Backend.API.Dtos;

public record class AuthResponseDto(string Token, string RefreshToken);