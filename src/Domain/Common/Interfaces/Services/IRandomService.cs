namespace Domain.Common.Interfaces.Services;


public interface IRandomService
{
    string GenerateToken(int length);
}

