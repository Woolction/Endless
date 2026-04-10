using Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Application.Users.Update;

public record class UserUpdateRequest(
    string? Name, string? Description, UserRole Role, IFormFile? AvatarPhoto);