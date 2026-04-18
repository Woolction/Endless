using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Users.Delete;

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
        User? user = await context.Users.FindAsync(
            cmd.UserId, cancellationToken);

        if (user == null)
        {
            return Result<Null>.Failure(404, "User Not Found");
        }

        context.Users.Remove(user);

        await context.SaveChangesAsync();

        await userRepository.DeleteSearchIndex(cmd.UserId, cancellationToken);

        logger.LogInformation("User {UserId} deleted", cmd.UserId);

        return Result<Null>.Success(204, new Null());
    }
}