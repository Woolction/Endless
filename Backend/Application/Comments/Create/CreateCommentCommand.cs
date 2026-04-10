using MediatR;

namespace Application.Comments.Create;

public record class CreateCommentCommand(
    string Text) : IRequest;