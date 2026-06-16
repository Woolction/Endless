using System.Text.Json;
using Application.Features.Rows;
using Application.Features.Rows.Users;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Services;
using Application.Utilities;
using Application.Interfaces.Db;
using Domain.Entities;
using MediatR;
using Npgsql;

namespace Application.Features.Users.Create.Registry;

public class UserRegistryHandler : IRequestHandler<UserRegistryCommand, Result<RegistryDto>>
{
    private readonly SearchIndexUpsertPublisher indexUpsertPublisher;
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly ILogger<UserRegistryHandler> logger;
    private readonly IAuthService authService;
    private readonly IAppDbContext context;

    public UserRegistryHandler(IPasswordHasher<User> passwordHasher, IAuthService authService, IAppDbContext context, ILogger<UserRegistryHandler> logger, SearchIndexUpsertPublisher indexUpsertPublisher)
    {
        this.indexUpsertPublisher = indexUpsertPublisher;
        this.passwordHasher = passwordHasher;
        this.authService = authService;
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<RegistryDto>> Handle(UserRegistryCommand cmd, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(user => user.Email == cmd.Email, cancellationToken))
            return Result<RegistryDto>.Failure(409, $"User with Email: {cmd.Email} exists");

        User user = new()
        {
            RegistryData = DateTime.UtcNow,
            DateOfBirth = DateTime.UtcNow,
            IsWound = false
        };

        user.SetPassword(passwordHasher.HashPassword(user, cmd.Password));
        user.SetEmail(cmd.Email);

        if (!string.IsNullOrEmpty(cmd.Name))
        {
            user.SetName(cmd.Name);
            user.SetSlug(cmd.Name.GenerateSlug());
        }

        var vectors = await context.Genres
            .Select(genre => new UserGenreVector()
            {
                User = user,
                GenreId = genre.Id
            })
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        context.UserVectors.AddRange(vectors);
        context.Users.Add(user);

        await context.SaveChangesAsync();

        string[] tokens = await authService.CreateTokenResponse(user);

        if (tokens.Length != 2)
            return Result<RegistryDto>.Failure(500, "Token could not be created");

        logger.LogInformation("User {UserId} registred",
            user.Id);

        var meta = await context.UserMetas
            .AsNoTracking()
            .Include(u => u.Image)
                .ThenInclude(i => i.Variants)
            .FirstAsync(u => u.UserId == user.Id, cancellationToken);

        await indexUpsertPublisher.Publish(
            new SearchIndexUpsertMessage(nameof(User), 
                JsonSerializer.Serialize(new UserSearchIndex(user, meta.Image, meta.Image.Variants))), 
            cancellationToken);

        return Result<RegistryDto>.Success(201, new RegistryDto(user.Id, tokens[0], tokens[1]));
    }
}