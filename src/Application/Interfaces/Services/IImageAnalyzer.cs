using Application.Features.Images;
using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IImageAnalyzer
{
    Task SetImageVariants(Image image, ImageDto variantsDto, CancellationToken token);
    Task SetAverageColor(string photoUrl, Action<int, int, int> setColor, CancellationToken token = default);
    Task<ImageDto> GenerateImageVariantsDto(string photoPath, string folder, (int w, int h)[] sizes, int quality = 80, CancellationToken token = default);
}