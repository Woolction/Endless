using Application.Features.Authentications.Dtos;
using MediatR;

namespace Application.Features.Authentications.Update;

public record class RefreshTokenCommand(
    string Token) : IRequest<Result<AuthDto>>;