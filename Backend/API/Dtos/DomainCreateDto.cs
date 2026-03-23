namespace Backend.API.Dtos;

public record class DomainCreateDto(
    string Name, IFormFile? AvatarPhoto);