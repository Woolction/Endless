using Application.Users.Dtos;
using MediatR;

namespace Application.Users.Changed;

public record class UserChangedQuery(Guid UserId) : IRequest<Result<UserDto>>;