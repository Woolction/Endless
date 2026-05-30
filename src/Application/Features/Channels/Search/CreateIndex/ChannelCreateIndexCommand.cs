using Application.Features.Searchs;
using MediatR;

namespace Application.Features.Channels.Search.CreateIndex;

public record class ChannelCreateIndexCommand() : IRequest<Result<IndexCreatedDto>>;