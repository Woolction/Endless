using Domain.Interfaces.Repositories;
using Application.Searchs;
using Application.Users.Dtos;
using Contracts.Rows;
using MediatR;

namespace Application.Users.Search;

public class UserSearchingHandler : IRequestHandler<UserSearchQuery, Result<UserSearchDto>>
{
    private readonly IUserRepository userRepository;

    public UserSearchingHandler(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<Result<UserSearchDto>> Handle(UserSearchQuery query, CancellationToken cancellationToken)
    {
        bool hasLastSearch = query.LastSearch != null;

        SearchDto searchDto = hasLastSearch == true ? query.LastSearch! : new SearchDto();

        IEnumerable<UserSearchRow> result = await userRepository.SearchUsersByName(
            query.Name, hasLastSearch, searchDto.LastScore, searchDto.LastId, cancellationToken);

        UserDto[] users = result.Select(u => new UserDto(
            u.Id, u.Name, "@" + u.Slug, u.Description ?? "",
            u.RegistryData, u.Email, u.Role.ToString(),
            u.AvatarPhotoUrl, u.TotalLikes, u.CommentsCount,
            u.ContentsCount, u.FollowersCount, u.FollowingCount,
            u.OwnedChannelsCount, u.ChannelSubscriptionsCount
        )).ToArray();

        if (users.Length < 1)
            return Result<UserSearchDto>.Failure(404, $"User with name: {query.Name} not found");

        var last = result.Last();

        SearchDto lastSearch = new()
        {
            LastScore = last.Score,
            LastId = last.Id
        };

        return Result<UserSearchDto>.Success(200, new UserSearchDto(users, lastSearch));
    }
}