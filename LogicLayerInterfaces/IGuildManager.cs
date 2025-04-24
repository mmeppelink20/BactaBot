using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IGuildManager
    {
        Task AddGuildAsync(SocketGuild guilds);
        Task AddMultipleGuildsAsync(IReadOnlyCollection<SocketGuild> guilds);
        Task DeactivateMultipleGuildsAsync(IReadOnlyCollection<SocketGuild> guilds);
        Task DeleteGuildAsync(SocketGuild guild);
        Task RegisterGuildsAsync();
        Task UpdateGuildAsync(SocketGuild guild);
    }
}
