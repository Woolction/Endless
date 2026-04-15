using Application.Users.Create.Many;
using Application.Users.Dtos;
using Application.Utilities;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Application.Users.Create.Many;

public class UsersCreatingHandler : IRequestHandler<UsersCreateCommand, Result<UserDto[]>>
{
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IUserRepository userRepository;
    private readonly IAppDbContext context;

    public UsersCreatingHandler(IAppDbContext context, IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        this.passwordHasher = passwordHasher;
        this.userRepository = userRepository;
        this.context = context;
    }

    public async Task<Result<UserDto[]>> Handle(UsersCreateCommand cmd, CancellationToken cancellationToken)
    {
        List<User> users = new();
        List<UserGenreVector> vectors = new();

        var genres = await context.Genres
                .Select(genre => genre.Id)
                .ToArrayAsync(cancellationToken);

        for (int i = 0; i < cmd.Names.Length; i++)
        {
            User user = new()
            {
                RegistryData = DateTime.UtcNow,
                IsWound = true
            };

            user.SetName(cmd.Names[i]);
            user.SetSlug(cmd.Names[i].GenerateSlug());
            user.SetEmail(cmd.Names[i] + "@gmail.com");
            user.SetPassword($"{cmd.Password}");
            //user.SetPassword(passwordHasher.HashPassword(user, cmd.Password));

            for (int j = 0; j < genres.Length; j++)
            {
                vectors.Add(new UserGenreVector()
                {
                    User = user,
                    GenreId = genres[j]
                });
            }

            await userRepository.CreateSearchIndex(user);

            users.Add(user);
        }

        context.Users.AddRange(users);
        context.UserVectors.AddRange(vectors);

        try
        {
            await context.SaveChangesAsync();

            return Result<UserDto[]>.Success(201, users.Select(user => new UserDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, 0, 0, 0, 0, 0, 0, 0)).ToArray());
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                Result<UserDto[]>.Failure(409, "User name already exists");

            throw;
        }
    }
}