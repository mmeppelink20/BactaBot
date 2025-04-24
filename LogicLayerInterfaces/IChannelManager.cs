using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IChannelManager
    {
        Task AddChannelAsync(SocketGuildChannel channel);
        Task AddMultipleChannelsAsync(IReadOnlyCollection<SocketGuild> guilds);
        Task DeleteChannelAsync(ulong channelId);
        Task UpdateChannelAsync(ulong channelId, string channelName);
        Task DeactivateMultipleChannelsAsync(IReadOnlyCollection<SocketGuild> guilds);
        Task RegisterChannelsAsync();

    }
}
