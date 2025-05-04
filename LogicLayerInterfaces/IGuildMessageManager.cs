using DataObjects;
using Discord;
using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IGuildMessageManager
    {
        Task AddDiscordMessageAsync(SocketMessage message);
        Task<List<DiscordMessageVM>> RetrieveDiscordMessagesAsync();
        Task<List<DiscordMessageVM>> RetrieveDiscordMessagesByChannelIDAndMinutesAsync(ulong channelID, int minutes);
        Task<bool> DeleteDiscordMessageAsync(ulong messageID);
    }
}
