using Application.Dtos.Genres;
using Domain.Entities;

namespace API.Extensions;

public static class VectorsExtension
{
    public static UserGenreVectorDto[] GetUserGenreVectors(this UserGenreVector[] userGenres)
    {
        UserGenreVectorDto[] userGenresVectors = new UserGenreVectorDto[userGenres.Length - 1];

        for (int i = 0; i < userGenres.Length; i++)
        {
            userGenresVectors[i] = userGenres[i].GetUserGenreVector();
        }

        return userGenresVectors;
    }

    public static UserGenreVectorDto GetUserGenreVector(this UserGenreVector userGenre)
    {
        return new UserGenreVectorDto(userGenre.Genre!.GetGenreDto(), userGenre.Value);
    }

    public static ContentGenreVectorDto[] GetContentGenreVectors(this ContentGenreVector[] contentGenres)
    {
        ContentGenreVectorDto[] contentGenresVectors = new ContentGenreVectorDto[contentGenres.Length - 1];

        for (int i = 0; i < contentGenres.Length; i++)
        {
            contentGenresVectors[i] = contentGenres[i].GetContentGenreVector();
        }

        return contentGenresVectors;
    }

    public static ContentGenreVectorDto GetContentGenreVector(this ContentGenreVector contentGenre)
    {
        return new ContentGenreVectorDto(
            contentGenre.Genre!.GetGenreDto(),
            contentGenre.AuthorVector,
            contentGenre.AudienceVector,
            contentGenre.FinalVector
            );
    }
}