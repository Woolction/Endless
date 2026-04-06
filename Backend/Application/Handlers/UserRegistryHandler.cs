using Application.Commands.Authentications;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces.Services;
using Contracts.Dtos.Users;
using Application.Utilities;
using Domain.Entities;
using Npgsql;

namespace Application.Handlers;

public class UserRegistryHandler
{
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IUserVectorsRepository userVectors;
    private readonly IGenreRepository genreRepository;
    private readonly IUserRepository userRepository;
    private readonly IAuthService authService;

    public UserRegistryHandler(IPasswordHasher<User> passwordHasher, IUserVectorsRepository userVectors, IGenreRepository genreRepository, IUserRepository userRepository, IAuthService authService)
    {
        this.genreRepository = genreRepository;
        this.passwordHasher = passwordHasher;
        this.userRepository = userRepository;
        this.userVectors = userVectors;
        this.authService = authService;
    }

    public async Task<Result<RegistryDto>> Handle(AuthCreateCommand cmd)
    {
        if (await userRepository.AnyUserByEmail(cmd.Email))
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

        var vectors = await genreRepository.CreateGenresForUser(user);

        await userVectors.AddExistsGenres(vectors);

        userRepository.AddUser(user);

        try
        {
            await userRepository.SaveChangesAsync();

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