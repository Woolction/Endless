using Domain.Entities;

namespace Application.Queries.Contents;

public record class ContentRecoScoreQuery(
    Content Content, float Score);