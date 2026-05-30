using MediatR;

namespace Application.Features.Comments.Create;

public record class CreateCommentCommand(
    string Text) : IRequest;