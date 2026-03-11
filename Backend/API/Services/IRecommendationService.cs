using Backend.API.Data.Models;

namespace Backend.API.Services;

public interface IRecommendationService
{
    float Recommend(User user, Content content);
}