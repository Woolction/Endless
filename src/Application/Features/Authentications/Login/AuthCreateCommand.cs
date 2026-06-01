using Application.Features.Authentications.Dtos;
using MediatR;

namespace Application.Features.Authentications.Login;

public record class AuthCreateCommand(
    string? Name, string Email, string Password) : IRequest<Result<AuthDto>>;