using Application.Dtos.Users;

namespace Application.Dtos.Comments;

public record class CommentSendedDto(
    CommentDto CommentDto, UserDto User);