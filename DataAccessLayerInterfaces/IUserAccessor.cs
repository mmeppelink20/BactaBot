using DataObjects;

namespace DataAccessLayerInterfaces
{
    public interface IUserAccessor
    {
        Task InsertMultipleUsersAsync(Dictionary<ulong, User> users);
        Task InsertGuildUsers(Dictionary<ulong, List<GuildUser>> users);
        Task DeactivateGuildUsers(Dictionary<ulong, List<GuildUser>> users);
        Task DeactivateMultipleUsersAsync(Dictionary<ulong, User> users);

    }
}
