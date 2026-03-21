using Backend.API.Data.Components;
using Backend.API.Data.Models;

namespace Backend.API.Dtos;

public record class ContentCreateDto(IFormFile? ContentFile, IFormFile? PrewievPhoto,
    string Title, string? Description, ContentType ContentType);