using Application.Users.Dtos;

namespace Application.Comments.Dtos;

public record class CommentSendedDto(
    CommentDto CommentDto, UserDto User);