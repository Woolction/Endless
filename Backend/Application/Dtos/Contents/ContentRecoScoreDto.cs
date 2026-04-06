using Domain.Entities;

namespace Application.Dtos.Contents;

public record class ContentRecoScoreQuery(
    Content Content, float Score);