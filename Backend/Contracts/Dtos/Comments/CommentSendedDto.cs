using Contracts.Dtos.Users;

namespace Contracts.Dtos.Comments;

public record class CommentSendedDto(
    CommentDto CommentDto, UserDto User);