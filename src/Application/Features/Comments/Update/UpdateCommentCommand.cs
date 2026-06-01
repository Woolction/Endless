using MediatR;

namespace Application.Features.Comments.Update;

public record class UpdateCommentCommand(
    string Text) : IRequest;

