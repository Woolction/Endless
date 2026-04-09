using Domain.Entities;

namespace Application.Contents.Recommendate;

public record class ContentRecoScoreQuery(
    Content Content, float Score);