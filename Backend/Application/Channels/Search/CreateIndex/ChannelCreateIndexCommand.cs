using Application.Searchs;
using MediatR;

namespace Application.Channels.Search.CreateIndex;

public record class ChannelCreateIndexCommand() : IRequest<Result<IndexCreatedDto>>;