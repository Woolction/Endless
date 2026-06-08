using Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Recommendation.Services;

namespace Recommendation;

public static class DependencyInjection
{
    public static void AddRecommendationInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRecommendationService, RecommendationService>();
        services.AddSingleton<IInteractionService, InteractionService>();
    }
}