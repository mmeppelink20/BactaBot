using DataAccessLayerInterfaces;
using DataObjects;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace LogicLayer
{
    public class GuildManager(ILogger<IGuildManager> logger, IConfiguration configuration, IGuildAccessor guildAccessor, DiscordSocketClient client) : IGuildManager
    {
        private readonly ILogger<IGuildManager> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IGuildAccessor _guildAccessor = guildAccessor;
        private readonly DiscordSocketClient _client = client;

        public async Task AddGuildAsync(SocketGuild guild)
        {
            try
            {
                await _guildAccessor.InsertGuildAsync(guild.Id, guild.Name);

                _logger.LogInformation((int)BactaLogging.LogEvent.Configuration, "Guild added to the database.");
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.Configuration, ex, "Failed to add guild to the database");
            }
        }


        public async Task AddMultipleGuildsAsync(IReadOnlyCollection<SocketGuild> guilds)
        {
            try
            {
                Dictionary<ulong, string> keyValuePairs = [];

                foreach (var guild in guilds)
                {
                    keyValuePairs[guild.Id] = guild.Name;
                }

                await _guildAccessor.InsertMultipleGuildsAsync(keyValuePairs);

                // log that the gulids were inserted and include a string of the guilds that were inserted
                _logger.LogInformation((int)BactaLogging.LogEvent.StartUpShutDown, "Guilds added to the database");
            }
            catch (DbException dbEx)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, dbEx, "Database error while adding guilds.");
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, ex, "Unexpected error while adding guilds.");
            }
        }

        public async Task DeactivateMultipleGuildsAsync(IReadOnlyCollection<SocketGuild> guilds)
        {
            try
            {
                List<ulong> guildIds = [];
                foreach (var guild in guilds)
                {
                    guildIds.Add(guild.Id);
                }
                await _guildAccessor.DeactivateMultipleGuildsAsync(guildIds);
                // log that the guilds were deactivated and include a string of the guilds that were deactivated
            }
            catch (DbException dbEx)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, dbEx, "Database error while deactivating guilds.");
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, ex, "Unexpected error while deactivating guilds.");
            }
        }

        public Task DeleteGuildAsync(SocketGuild guild)
        {
            throw new NotImplementedException();
        }

        public async Task RegisterGuildsAsync()
        {
            var guilds = _client.Guilds;

            await AddMultipleGuildsAsync(guilds);
            await DeactivateMultipleGuildsAsync(guilds);
        }

        public Task UpdateGuildAsync(SocketGuild guild)
        {
            throw new NotImplementedException();
        }
    }
}
