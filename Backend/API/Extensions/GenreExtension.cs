using Contracts.Dtos.Genres;
using Domain.Entities;

namespace API.Extensions;

public static class GenreExtension
{
    public static List<GenreDto> GetGenreResponsesDto(this List<Genre> genres)
    {
        List<GenreDto> genreResponses = new();

        for (int i = 0; i < genres.Count; i++)
        {
            genreResponses.Add(genres[i].GetGenreDto());
        }

        return genreResponses;
    }

    public static GenreDto GetGenreDto(this Genre genre)
    {
        return new GenreDto(genre.Id, genre.Name, genre.Order);
    }
}