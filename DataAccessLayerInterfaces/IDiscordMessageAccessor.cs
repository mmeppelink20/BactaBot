using DataObjects;
using Discord;
using Discord.WebSocket;

namespace DataAccessLayerInterfaces
{
    public interface IDiscordMessageAccessor
    {
        // insert
        Task InsertDiscordMessage(DiscordMessageVM message);

        // update
        Task UpdateDiscordMessage(DiscordMessage message);

        // delete
        Task<bool> DeleteDiscordMessage(ulong messageID);

        // select
        Task<SocketMessage> GetDiscordMessage(DiscordMessage message);

        // select by user, channel, and guild
        Task<List<SocketMessage>> GetDiscordMessagesByUserChannel(ulong userId, ulong channelId, ulong guildId);

        // get message count by user for the given message
        Task<Dictionary<string, int>> GetMessageCountByUser(string message);

        // get messages
        Task<List<DiscordMessageVM>> GetDiscordMessagesByChannelIDAndMinutesAsync(ulong channelID, int minutes);
    }
}
