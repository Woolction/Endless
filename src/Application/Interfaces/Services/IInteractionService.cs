using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IInteractionService
{
    void Interaction(UserGenreVector[] userVectors, Content content, ContentGenreVector[] contentVectors, UserInteractionContent interaction, int Count);
}