using Application.Features.Searchs;
using MediatR;

namespace Application.Features.Users.Search.CreateIndex;

public record class UserCreateIndexCommand() : IRequest<Result<IndexCreatedDto>>;