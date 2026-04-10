using Application.Authentications.Dtos;
using MediatR;

namespace Application.Authentications.Update;

public record class RefreshTokenCommand(
    string Token) : IRequest<Result<AuthDto>>;