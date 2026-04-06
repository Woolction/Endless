using Domain.Entities;

namespace Domain.Interfaces.Repositories;

public interface IUserVectorsRepository
{
    Task AddExistsGenres(UserGenreVector[] userGenres);
    Task<int> SaveChangesAsync();
}