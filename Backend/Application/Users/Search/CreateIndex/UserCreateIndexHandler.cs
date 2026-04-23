using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch;
using Application.Searchs;
using Domain.Rows.Users;
using MediatR;
using System.Runtime.CompilerServices;
using Domain.Interfaces.Repositories;

namespace Application.Users.Search.CreateIndex;

public class UserCreateIndexHandler : IRequestHandler<UserCreateIndexCommand, Result<IndexCreatedDto>>
{
    private readonly IUserRepository userRepository;
    public UserCreateIndexHandler(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<Result<IndexCreatedDto>> Handle(UserCreateIndexCommand request, CancellationToken cancellationToken)
    {
        var response = await userRepository.CreateMapping(cancellationToken);

       

        if (!response.IsValidResponse || !response.IsSuccess())
            return Result<IndexCreatedDto>.Failure(500, response.DebugInformation);

        return Result<IndexCreatedDto>.Success(201, new IndexCreatedDto(
            response.DebugInformation));
    }
}