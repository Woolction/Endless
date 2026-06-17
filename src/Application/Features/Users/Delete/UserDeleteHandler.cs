using Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Db;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Delete;

public class UserDeleteHandler : IRequestHandler<UserDeleteCommand, Result<Null>>
{
    private readonly ILogger<UserDeleteHandler> logger;
    private readonly IUserRepository userRepository;
    private readonly IAppDbContext context;

    public UserDeleteHandler(IAppDbContext context, IUserRepository userRepository, ILogger<UserDeleteHandler> logger)
    {
        this.userRepository = userRepository;
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<Null>> Handle(UserDeleteCommand cmd, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Where(user => user.Id == cmd.UserId)
            .Select(user => new { user, iconsPath = user.UserMeta.Image.BaseUrl })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return Result<Null>.Failure(404, "User Not Found");

        if (!string.IsNullOrEmpty(user.iconsPath) && Directory.Exists(user.iconsPath))
            Directory.Delete(user.iconsPath, true);

        context.Users.Remove(user.user);

        await context.SaveChangesAsync();

        await userRepository.DeleteSearchIndex(
            cmd.UserId, cancellationToken);

        logger.LogInformation("User {UserId} deleted", cmd.UserId);

        return Result<Null>.Success(204, new Null());
    }
}