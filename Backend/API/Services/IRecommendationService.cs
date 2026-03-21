using Backend.API.Data.Models;

namespace Backend.API.Services;

public interface IRecommendationService
{
    float Recommend(UserGenreVector[] userGenres, Content content, VideoMetaData videoMeta, ContentGenreVector[] contentGenres, int vectorsCount);
}