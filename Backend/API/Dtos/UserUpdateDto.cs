using Backend.API.Data.Components;

namespace Backend.API.Dtos;

public record class UserUpdateDto(string Name, string Description, UserRole Role);