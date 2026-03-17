using Backend.API.Data.Models;
using Backend.API.Dtos;

namespace Backend.API.Extensions;

public static class GenreExtension
{
    public static List<GenreResponseDto> GetGenreResponsesDto(this List<Genre> genres)
    {
        List<GenreResponseDto> genreResponses = new();

        for (int i = 0; i < genres.Count; i++)
        {
            genreResponses.Add(genres[i].GetGenreResponseDto());
        }

        return genreResponses;
    }

    public static GenreResponseDto GetGenreResponseDto(this Genre genre)
    {
        return new GenreResponseDto(genre.Id, genre.Name, genre.Order);
    }
}