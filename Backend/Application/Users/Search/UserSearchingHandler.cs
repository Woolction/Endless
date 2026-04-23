using Domain.Interfaces.Repositories;
using Application.Searchs;
using Application.Users.Dtos;
using MediatR;
using Domain.Rows.Users;
using Microsoft.Extensions.Logging;
using Elastic.Clients.Elasticsearch;

namespace Application.Users.Search;

public class UserSearchingHandler : IRequestHandler<UserSearchQuery, Result<SearchedUserDto[]>>
{
    private readonly ILogger<UserSearchingHandler> logger;
    private readonly IUserRepository userRepository;

    public UserSearchingHandler(IUserRepository userRepository, ILogger<UserSearchingHandler> logger)
    {
        this.userRepository = userRepository;
        this.logger = logger;
    }

    public async Task<Result<SearchedUserDto[]>> Handle(UserSearchQuery query, CancellationToken cancellationToken)
    {
        ICollection<FieldValue> lastValue = [];

        if (query.LastScore != null)
            lastValue.Add(FieldValue.Double(
                query.LastScore.Value)); 

        UserSearchRow result = await userRepository.SearchUsersByName(
            query.Name, lastValue, cancellationToken);

        if (result.SearchedUsers.Count < 1)
            return Result<SearchedUserDto[]>.Failure(404, $"User with name: {query.Name} not found: returned: {result.SearchedUsers.Count}");

        SearchedUserDto[] userDtos = result.SearchedUsers.Select(u => new SearchedUserDto(new UserDto(
            u.SearchedUser.UserId, u.SearchedUser.Name, "@" + u.SearchedUser.Slug, u.SearchedUser.Description,
            u.SearchedUser.RegistryData, u.SearchedUser.Email, u.SearchedUser.Role.ToString(),
            u.SearchedUser.AvatarPhotoUrl, u.SearchedUser.TotalLikes, 0, 0, 0, 0, 0, 0 
            /*u.CommentsCount, u.ContentsCount, u.FollowersCount, u.FollowingCount,
            u.OwnedChannelsCount, u.ChannelSubscriptionsCount*/
        ), u.Score)).ToArray();

        logger.LogInformation("Search returned users: {Count} results for {Query}",
            userDtos.Length, query.Name);

        return Result<SearchedUserDto[]>.Success(200, userDtos);
    }
}