using Backend.API.Data.Models;

namespace Backend.API.Dtos;

public record class ContentRecoScoreDto(
    Content Content, float Score);