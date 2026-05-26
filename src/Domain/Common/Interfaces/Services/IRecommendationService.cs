using Domain.Entities;

namespace Domain.Common.Interfaces.Services;

public interface IRecommendationService
{
    float Recommend(UserGenreVector[] userGenres, Content content, VideoMeta? videoMeta, ContentGenreVector[] contentGenres, int vectorsCount);
}