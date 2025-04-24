namespace DataAccessLayerInterfaces
{
    public interface IGuildAccessor
    {
        Task DeleteGuildAsync(ulong guildId);
        Task InsertGuildAsync(ulong guildId, string guildName);
        Task InsertMultipleGuildsAsync(Dictionary<ulong, string> guilds);
        Task DeactivateMultipleGuildsAsync(List<ulong> guilds);
        Task UpdateGuildAsync(ulong guildId, string guildName);

    }
}
