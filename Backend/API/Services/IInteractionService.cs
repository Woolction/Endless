using Backend.API.Data.Models;

namespace Backend.API.Services;

public interface IInteractionService
{
    void Interaction(User user, Content content, UserInterationContent interaction);
}