using DataObjects;
using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IUserManager
    {
        Task AddUserAsync(SocketUser user);
        Task AddMultipleUsersAsync(Dictionary<ulong, User> users);
        Task DeactivateMultipleUsersAsync(Dictionary<ulong, User> users);
        Task AddGuildUsersAsync(Dictionary<ulong, List<GuildUser>> guildUsers);
        Task DeactivateGuildUsersAsync(Dictionary<ulong, List<GuildUser>> guildUsers);
        Task RegisterUsersAsync();
        Task RegisterGuildUsersAsync();
    }
}
