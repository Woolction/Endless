using Domain.Rows.Channels;

namespace Domain.Interfaces.Repositories;

public interface IChannelRepository
{
    Task<IEnumerable<ChannelSearchRow>> SearchChannelsByName(string name, bool hasLastSearch, double lastScore, Guid lastId, CancellationToken token);
}