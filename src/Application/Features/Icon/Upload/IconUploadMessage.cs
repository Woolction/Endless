using Domain.Common.Enums;
using MediatR;

namespace Application.Features.Icon.Upload;

public record class IconUploadMessage(
    Guid Id, IconType Type, string Slug, string PhotoPath) : IRequest<Result<Null>>;