using Application.Commands.Users;
using Application.Dtos.Users;
using Application.Utilities;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Application.Handlers;

public class UsersCreatingHandler
{
    private readonly ILogger<UsersCreatingHandler> logger;
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IAppDbContext context;

    public UsersCreatingHandler(ILogger<UsersCreatingHandler> logger, IAppDbContext context, IPasswordHasher<User> passwordHasher)
    {
        this.passwordHasher = passwordHasher;
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<UserDto[]>> Handle(UsersCreateCommand cmd)
    {
        List<User> users = new();
        List<UserGenreVector> vectors = new();

        var genres = await context.Genres
                .Select(genre => genre.Id)
                .ToArrayAsync();

        for (int i = 0; i < cmd.Count; i++)
        {
            string IdForName = Guid.CreateVersion7().ToString();

            User user = new();

            user.SetName(IdForName);
            user.SetSlug(IdForName.GenerateSlug());
            user.SetEmail(IdForName + "@gmail.com");
            user.RegistryData = DateTime.UtcNow;
            //user.SetPassword(passwordHasher.HashPassword(user, cmd.Password));

            user.SetPassword($"{cmd.Password}");

            users.Add(user);

            for (int j = 0; j < genres.Length; j++)
            {
                vectors.Add(new UserGenreVector()
                {
                    User = user,
                    GenreId = genres[j]
                });
            }
        }

        context.Users.AddRange(users);
        context.UserVectors.AddRange(vectors);

        try
        {
            await context.SaveChangesAsync();

            return Result<UserDto[]>.Success(201, users.Select(user => new UserDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, 0,0,0,0,0,0,0)).ToArray());
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                Result<UserDto[]>.Failure(409, "User name already exists");

            throw;
        }
    }
}