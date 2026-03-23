using Backend.API.Data.Components;

namespace Backend.API.Dtos;

public record class ContentCreateDto(
    IFormFile? ContentFile, IFormFile? PrewievPhoto,
    string Title, string? Description, ContentType ContentType);