using Application.Searchs;
using MediatR;

namespace Application.Users.Search.CreateIndex;

public record class UserCreateIndexCommand() : IRequest<Result<IndexCreatedDto>>;