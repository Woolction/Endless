using Backend.API.Data.Models;
using Backend.API.Extensions;

namespace Backend.API.Services;

public class InteractionService : IInteractionService
{
    //call when user exits the video
    public void Interaction(User user, Content content, UserInterationContent interaction)
    {
        VideoMetaData videoMeta = content.VideoMeta!;

        if (videoMeta.DurationSeconds == 0)
            return;

        float watchRatio =
            (float)interaction.WatchTimeSeconds /
            videoMeta.DurationSeconds; //! for test

        watchRatio = MathF.Min(watchRatio, 1f); 

        float weight =
            0.7f * watchRatio +
            0.2f * (interaction.Liked ? 1 : 0) +
            0.1f * (interaction.Saved ? 1 : 0);

        UpdateUserVector(user, content, weight);
        UpdatecontentAudienceVector(user, content, weight);

        UpdateFinalcontentVector(content);

        UpdateWatchStats(content, interaction.WatchTimeSeconds);
    }

    private void UpdateUserVector(User user, Content content, float weight)
    {
        //if (!user.AutoLearningEnabled) return; opcinional

        for (int i = 0; i < user.VectorsCount; i++)
        {
            user.Vectors[i].Value =
                0.9f * user.Vectors[i].Value +
                0.1f * weight * content.Vectors[i].FinalVector;
        }

        VectorManager.Normalize(
            user.Vectors, user.VectorsCount, x => x.Value, (x, value) => x.Value = value);
    }

    private void UpdatecontentAudienceVector(User user, Content content, float weight)
    {
        for (int i = 0; i < content.VectorsCount; i++)
        {
            content.Vectors[i].AudienceVector += weight * user.Vectors[i].Value;
        }

        VectorManager.Normalize(
            content.Vectors, content.VectorsCount, x => x.AudienceVector, (x, value) => x.AudienceVector = value);
    }

    private void UpdateFinalcontentVector(Content content)
    {
        for (int i = 0; i < content.VectorsCount; i++)
        {
            content.Vectors[i].FinalVector =
                0.5f * content.Vectors[i].AuthorVector +
                0.5f * content.Vectors[i].AudienceVector;
        }

        VectorManager.Normalize(
            content.Vectors, content.VectorsCount, x => x.FinalVector, (x, value) => x.FinalVector = value);
    }

    private void UpdateWatchStats(Content content, int watchTimeSeconds)
    {
        VideoMetaData videoMeta = content.VideoMeta!; // for test

        float watchRatio =
            (float)watchTimeSeconds / videoMeta.DurationSeconds;

        watchRatio = MathF.Min(watchRatio, 1f);

        videoMeta.AverageWatchTimeSeconds =
            (int)(videoMeta.AverageWatchTimeSeconds * content.ViewsCount + watchTimeSeconds)
            / (int)(content.ViewsCount + 1);

        videoMeta.AverageWatchRatio =
            (videoMeta.AverageWatchRatio * content.ViewsCount + watchRatio)
            / (content.ViewsCount + 1);
    }
}