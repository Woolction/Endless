using Backend.API.Data.Components;

namespace Backend.API.Dtos;

public record class UserResponseDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    DateTime RegistryDate,
    string Email, UserRole Role);