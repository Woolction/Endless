using Application.Features.Users.Dtos;
using Microsoft.AspNetCore.Http;
using Domain.Common.Enums;
using MediatR;

namespace Application.Features.Users.Update;

public record class UserUpdateCommand(
    Guid UserId, string? Name, string? Description, UserRole Role, IFormFile? IconPhoto) : IRequest<Result<UserDto>>;