using Application.Users.Dtos;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Users.Update;

public record class UserUpdateCommand(
    Guid UserId, string? Name, string? Description, UserRole Role, IFormFile? AvatarPhoto) : IRequest<Result<UserDto>>;