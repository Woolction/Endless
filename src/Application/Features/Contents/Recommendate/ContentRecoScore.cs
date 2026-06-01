using Domain.Entities;

namespace Application.Features.Contents.Recommendate;

public record class ContentRecoScore(
    Content Content, float Score);