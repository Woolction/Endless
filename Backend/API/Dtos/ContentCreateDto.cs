using Backend.API.Data.Components;
using Backend.API.Data.Models;

namespace Backend.API.Dtos;

public record class ContentCreateDto(
    string Title, string? Description);