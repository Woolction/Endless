
using Domain.Entities;

namespace Domain.Interfaces.Repositories;

public interface IGenreRepository
{
    Task<UserGenreVector[]> CreateGenresForUser(User user);
    Task<ContentGenreVector[]> CreateGenresForContent(Content content);
}