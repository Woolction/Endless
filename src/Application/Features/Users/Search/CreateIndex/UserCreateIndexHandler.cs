using Application.Features.Searchs;
using Domain.Common.Interfaces.Repositories;
using Elastic.Clients.Elasticsearch;
using MediatR;

namespace Application.Features.Users.Search.CreateIndex;

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