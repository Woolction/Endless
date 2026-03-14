using Backend.API.Data.Models;

namespace Backend.API.Services;

public interface IRecommendationService
{
    float Recommend(UserGenreVector[] userGenres, Content content, ContentGenreVector[] contentGenres, int vectorsCount);
}