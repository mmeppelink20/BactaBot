namespace DataAccessLayerInterfaces
{
    public interface IChannelAccessor
    {
        Task InsertChannelAsync(ulong channelId, string channelName);
        Task DeleteChannelAsync(ulong channelId);
        Task UpdateChannelAsync(ulong channelId, string channelName);
        Task InsertMultipleChannelsAsync(List<(ulong guildId, ulong channelId, string channelName, string channelType)> channels);
        Task DeactivateMultipleChannelsAsync(List<ulong> channels);
    }
}
