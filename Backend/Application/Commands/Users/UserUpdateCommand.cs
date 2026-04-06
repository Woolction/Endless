using Domain.Components;
using Microsoft.AspNetCore.Http;

namespace Application.Commands.Users;

public record class UserUpdateCommand(
    string? Name, string? Description, UserRole Role, IFormFile? AvatarPhoto);