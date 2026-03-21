using Backend.API.Data.Models;
using Backend.API.Extensions;

namespace Backend.API.Services;

public class RecommendationService : IRecommendationService
{
    public float Recommend(UserGenreVector[] userGenres, Content content, VideoMetaData videoMeta, ContentGenreVector[] contentGenres, int vectorsCount)
    {
        float similarity = VectorManager.CosineSimilarity(
            userGenres, contentGenres, vectorsCount, x => x.Value, b => b.FinalVector);

        float trending = CalculateTrending(content, videoMeta);

        float freshness = CalculateFreshness(content);

        float quality = CalculateQuality(content, videoMeta);

        return
            0.55f * similarity +
            0.25f * trending +
            0.10f * freshness +
            0.10f * quality;
    }

    private float CalculateTrending(Content content, VideoMetaData videoMeta)
    {
        float hours =
            (float)(DateTime.UtcNow - content.CreatedDate).TotalHours;

        float viewsPerHour =
            content.ViewsCount / (hours + 1);

        float viewsScore =
            MathF.Log(viewsPerHour + 1);

        float likeScore =
            WilsonScore(content.LikesCount, content.ViewsCount);

        float watchScore =
            videoMeta.AverageWatchRatio;

        return viewsScore * likeScore * watchScore;
    }

    private float CalculateFreshness(Content content)
    {
        float hours = (float)(DateTime.UtcNow - content.CreatedDate).TotalHours;

        float decayConstant = 48f;

        float freshness =  MathF.Exp(-hours / decayConstant);

        if (freshness < 0)
            freshness = 0;

        return freshness;
    }

    private float CalculateQuality(Content content, VideoMetaData videoMeta)
    {
        float likeScore =
            WilsonScore(content.LikesCount, content.ViewsCount);

        return
            0.4f * likeScore +
            0.6f * videoMeta.AverageWatchRatio;
    }

    private float WilsonScore(long likes, long views)
    {
        if (views == 0)
            return 0;

        float z = 1.96f;
        float n = views;
        float p = (float)likes / views;

        float numerator =
            p + z * z / (2 * n) - z * MathF.Sqrt(
                (p * (1 - p) + z * z / (4 * n)) / n);

        float denominator = 1 + z * z / n;

        return numerator / denominator;
    }
}


