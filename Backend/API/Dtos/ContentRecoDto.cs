using Backend.API.Data.Components;

namespace Backend.API.Dtos;

public record class ContentRecoDto(
    string Title,
    string Slug,
    string? Description,
    DateTime CreatedDate,
    ContentType ContentType,
    double RandomKey);