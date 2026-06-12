using Application.Features.Rows.Contents;
using Application.Features.Rows.Users;
using Application.Features.Users.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Application.Features.Images;
using Application.Features.Dtos;
using Application.Features.Rows;
using Application.Interfaces.Db;
using Application.Utilities;
using System.Text.Json;
using Domain.Entities;
using MediatR;
using Npgsql;

namespace Application.Features.Users.Create.Many;

public class UsersCreatingHandler : IRequestHandler<UsersCreateCommand, Result<UserDto[]>>
{
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly SearchIndexUpsertPublisher indexUpsertPublisher;
    private readonly IAppDbContext context;

    public UsersCreatingHandler(IAppDbContext context, SearchIndexUpsertPublisher indexUpsertPublisher, IPasswordHasher<User> passwordHasher)
    {
        this.passwordHasher = passwordHasher;
        this.indexUpsertPublisher = indexUpsertPublisher;
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
                DateOfBirth = DateTime.UtcNow,
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

            users.Add(user);
        }

        context.Users.AddRange(users);
        context.UserVectors.AddRange(vectors);

        try
        {
            await context.SaveChangesAsync();

            var dtos = new UserDto[users.Count];

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];

                var meta = await context.UserMetas
                    .AsNoTracking()
                    .FirstAsync(u => u
                        .UserId == user.Id,
                        cancellationToken);

                dtos[i] = new UserDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData,
                    user.Email, user.Role.ToString(),
                    new PhotoDto(
                        new ImageVariants(
                            meta.IconBase,
                            meta.Small,
                            meta.Medium,
                            meta.Large),
                        meta.R,
                        meta.G,
                        meta.B),
                    0, 0, 0, 0, 0, 0, 0);

                await indexUpsertPublisher.Publish(
                    new SearchIndexUpsertMessage(nameof(User),
                        JsonSerializer.Serialize(new UserSearchIndex(user, meta))), cancellationToken);
            }

            return Result<UserDto[]>.Success(201, dtos);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                Result<UserDto[]>.Failure(409, "User name already exists");

            throw;
        }
    }
}