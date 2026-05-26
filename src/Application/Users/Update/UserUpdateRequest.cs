using Microsoft.AspNetCore.Http;
using Domain.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Application.Users.Update;

public record class UserUpdateRequest(
    string? Name, string? Description, UserRole Role, IFormFile? IconPhoto);