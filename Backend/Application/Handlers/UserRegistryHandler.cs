using Application.Commands.Authentications;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces.Services;
using Application.Utilities;
using Domain.Entities;
using Npgsql;
using Domain.Interfaces;
using Application.Dtos.Users;

namespace Application.Handlers;

public class UserRegistryHandler
{
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IAuthService authService;
    private readonly IAppDbContext context;

    public UserRegistryHandler(IPasswordHasher<User> passwordHasher, IAuthService authService, IAppDbContext context)
    {
        this.passwordHasher = passwordHasher;
        this.authService = authService;
        this.context = context;
    }

    public async Task<Result<RegistryDto>> Handle(AuthCreateCommand cmd)
    {
        if (await context.Users.AnyAsync(user => user.Email == cmd.Email))
            return Result<RegistryDto>.Failure(409, $"User with Email: {cmd.Email} exists");

        User user = new()
        {
            RegistryData = DateTime.UtcNow
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
            .ToArrayAsync();

        context.UserVectors.AddRange(vectors);

        context.Users.Add(user);

        try
        {
            await context.SaveChangesAsync();

            string[] tokens = await authService.CreateTokenResponse(user);

            if (tokens.Length != 2)
                return Result<RegistryDto>.Failure(500, "Token could not be created");

            return Result<RegistryDto>.Success(201, new RegistryDto(user.Id, tokens[0], tokens[1]));
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                Result<RegistryDto>.Failure(409, "User name already exists", 1);

            throw;
        }
    }
}