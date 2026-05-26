using Microsoft.AspNetCore.Http;
using Application.Users.Dtos;
using Domain.Common.Enums;
using MediatR;

namespace Application.Users.Update;

public record class UserUpdateCommand(
    Guid UserId, string? Name, string? Description, UserRole Role, IFormFile? IconPhoto) : IRequest<Result<UserDto>>;