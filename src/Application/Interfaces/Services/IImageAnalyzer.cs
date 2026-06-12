using Application.Features.Imagess;

namespace Application.Interfaces.Services;

public interface IImageAnalyzer
{
    Task SetAverageColor(string photoUrl, Action<int, int, int> setColor, CancellationToken token = default);
    Task<ImageVariants> GenerateImageVariants(string photoPath, string folder, (int w, int h)[] sizes, int quality = 80, CancellationToken token = default);
}