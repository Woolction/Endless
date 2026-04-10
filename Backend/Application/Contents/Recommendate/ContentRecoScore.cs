using Domain.Entities;

namespace Application.Contents.Recommendate;

public record class ContentRecoScore(
    Content Content, float Score);