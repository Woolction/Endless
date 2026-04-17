using Domain.Interfaces.Repositories;
using Application.Searchs;
using Application.Users.Dtos;
using MediatR;
using Domain.Rows.Users;
using Microsoft.Extensions.Logging;
using Elastic.Clients.Elasticsearch;

namespace Application.Users.Search;

public class UserSearchingHandler : IRequestHandler<UserSearchQuery, Result<UserSearchDto>>
{
    private readonly ILogger<UserSearchingHandler> logger;
    private readonly IUserRepository userRepository;

    public UserSearchingHandler(IUserRepository userRepository, ILogger<UserSearchingHandler> logger)
    {
        this.userRepository = userRepository;
        this.logger = logger;
    }

    public async Task<Result<UserSearchDto>> Handle(UserSearchQuery query, CancellationToken cancellationToken)
    {
        FieldValue[]? lastValues = query.LastValues;

        UserSearchRow result = await userRepository.SearchUsersByName(
            query.Name, lastValues, cancellationToken);

        if (result.SearchedUsers.Count < 1)
            return Result<UserSearchDto>.Failure(404, $"User with name: {query.Name} not found: returned: {result.SearchedUsers.Count}");

        UserDto[] users = result.SearchedUsers.Select(u => new UserDto(
            u.UserId, u.Name, "@" + u.Slug, u.Description ?? "",
            u.RegistryData, u.Email, u.Role.ToString(),
            u.AvatarPhotoUrl, u.TotalLikes, 0, 0, 0, 0, 0, 0 /*u.CommentsCount,
            u.ContentsCount, u.FollowersCount, u.FollowingCount,
            u.OwnedChannelsCount, u.ChannelSubscriptionsCount*/
        )).ToArray();


        /*List<object> objects = [];

        if (result.LastValues != null)
        {
            Console.WriteLine(result.LastValues.Length);

            for (int i = 0; i < result.LastValues.Length; i++)
            {
                var value = result.LastValues[i];

                Console.WriteLine(value);
                Console.WriteLine(value.GetType());

                objects.Add(value);
            }
        }   */ 

        SearchDto lastSearch = new()
        {
            Last = result.LastValues
        };

        logger.LogInformation("Search returned users: {Count} results for {Query}",
            users.Length, query.Name);

        return Result<UserSearchDto>.Success(200, new UserSearchDto(users, lastSearch));
    }
}