using Application.Features.Users.Dtos;

namespace Application.Features.Comments.Dtos;

public record class CommentSendedDto(
    CommentDto CommentDto, UserDto User);