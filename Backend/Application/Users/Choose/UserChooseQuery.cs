using Application.Users.Dtos;
using MediatR;

namespace Application.Users.Choose;

public record class UserChooseQuery(Guid UserId) : IRequest<Result<UserDto>>;