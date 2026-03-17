
namespace Backend.API.Dtos;

public record class SendCommentDto(
    CommentResponseDto CommentResponseDto,
    UserResponseDto User);