using Application.Features.Users.Dtos;
using MediatR;

namespace Application.Features.Users.Choose;

public record class UserChooseQuery(Guid UserId) : IRequest<Result<UserDto>>;