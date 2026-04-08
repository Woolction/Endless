using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Context;
using Domain.Entities;
using Infrastructure.Connector;

namespace Infrastructure.Repositories;

public class GenreRepository : IGenreRepository
{
    private DbConnectorFactory connector;
    public GenreRepository(DbConnectorFactory connector)
    {
        this.connector = connector;
    }
}