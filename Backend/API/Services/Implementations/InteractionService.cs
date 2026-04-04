using Backend.API.Services.Interfaces;
using Backend.API.Data.Models;
using Backend.API.Managers;

namespace Backend.API.Services.Implementations;

public class InteractionService : IInteractionService
{
    //call when user exits the video
    public void Interaction(UserGenreVector[] userVectors, Content content, ContentGenreVector[] contentVectors, UserInterationContent interaction, int Count)
    {
        VideoMetaData videoMeta = content.VideoMeta!;

        if (videoMeta.DurationSeconds == 0)
            return;

        float watchRatio =
            (float)interaction.WatchTimeSeconds / videoMeta.DurationSeconds; 

        watchRatio = MathF.Min(watchRatio, 1f); 

        float weight =
            0.7f * watchRatio +
            0.2f * (interaction.Liked ? 1 : 0) +
            0.1f * (interaction.Saved ? 1 : 0);

        UpdateUserVector(userVectors, content, weight, Count);
        UpdateContentAudienceVector(userVectors, contentVectors, weight, Count);

        UpdateFinalContentVector(contentVectors, Count);

        UpdateWatchStats(content, interaction.WatchTimeSeconds);
    }

    private void UpdateUserVector(UserGenreVector[] userVectors, Content content, float weight, int Count)
    {
        //if (!user.AutoLearningEnabled) return; opcinional

        for (int i = 0; i < Count; i++)
        {
            userVectors[i].Value =
                0.9f * userVectors[i].Value +
                0.1f * weight * content.Vectors[i].FinalVector;
        }

        VectorManager.Normalize(
            userVectors, Count, x => x.Value, (x, value) => x.Value = value);
    }

    private void UpdateContentAudienceVector(UserGenreVector[] userVectors, ContentGenreVector[] contentVectors, float weight, int Count)
    {
        for (int i = 0; i < Count; i++)
        {
            contentVectors[i].AudienceVector += weight * userVectors[i].Value;
        }

        VectorManager.Normalize(
            contentVectors, Count, x => x.AudienceVector, (x, value) => x.AudienceVector = value);
    }

    private void UpdateFinalContentVector(ContentGenreVector[] contentVectors, int Count)
    {
        for (int i = 0; i < Count; i++)
        {
            contentVectors[i].FinalVector =
                0.5f * contentVectors[i].AuthorVector +
                0.5f * contentVectors[i].AudienceVector;
        }

        VectorManager.Normalize(
            contentVectors, Count, x => x.FinalVector, (x, value) => x.FinalVector = value);
    }

    private void UpdateWatchStats(Content content, int watchTimeSeconds)
    {
        VideoMetaData videoMeta = content.VideoMeta!; // for test

        content.ViewsCount++;

        float watchRatio =
            (float)watchTimeSeconds / videoMeta.DurationSeconds;

        watchRatio = MathF.Min(watchRatio, 1f);

        videoMeta.AverageWatchTimeSeconds =
            (int)(videoMeta.AverageWatchTimeSeconds * content.ViewsCount + watchTimeSeconds)
            / (int)(content.ViewsCount);

        videoMeta.AverageWatchRatio =
            (videoMeta.AverageWatchRatio * content.ViewsCount + watchRatio)
            / (content.ViewsCount);
    }
}