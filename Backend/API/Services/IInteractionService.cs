using Backend.API.Data.Models;

namespace Backend.API.Services;

public interface IInteractionService
{
    void Interaction(UserGenreVector[] userVectors, Content content, ContentGenreVector[] contentVectors, UserInterationContent interaction, int Count);
}