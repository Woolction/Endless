namespace Application.Features.Users.Update;

public record class UserUpdateDto(
    Guid Id, string Name, string Slug, string? Description,
    DateTime RegistryDate, string Email, string Role, string Process);