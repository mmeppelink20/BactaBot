using DataAccessLayerInterfaces;
using DataObjects;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace LogicLayer
{
    public class ChannelManager(ILogger<IChannelManager> logger, IConfiguration configuration, IChannelAccessor channelAccessor, DiscordSocketClient client) : IChannelManager
    {
        private readonly ILogger<IChannelManager> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IChannelAccessor _channelAccessor = channelAccessor;
        private readonly DiscordSocketClient _client = client;
        public Task AddChannelAsync(SocketGuildChannel channel)
        {
            throw new NotImplementedException();
        }

        public async Task AddMultipleChannelsAsync(IReadOnlyCollection<SocketGuild> guilds)
        {
            try
            {
                List<(ulong GuildId, ulong ChannelId, string ChannelName, string ChannelType)> channelList = [];

                foreach (var guild in guilds)
                {
                    foreach (var channel in guild.Channels)
                    {
                        channelList.Add((guild.Id, channel.Id, channel.Name, channel.ChannelType.ToString()));
                    }
                }

                // Add the channels to the database
                await _channelAccessor.InsertMultipleChannelsAsync(channelList);

                _logger.LogInformation((int)BactaLogging.LogEvent.StartUpShutDown, "Channels added to the database.");
            }
            catch (DbException dbEx)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, dbEx, "Database error while inserting channels.");
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, ex, "Unexpected error while inserting channels.");
            }
        }

        public async Task DeactivateMultipleChannelsAsync(IReadOnlyCollection<SocketGuild> guilds)
        {
            try
            {
                List<ulong> channelList = [];
                foreach (var guild in guilds)
                {
                    foreach (var channel in guild.Channels)
                    {
                        channelList.Add(channel.Id);
                    }
                }
                // Deactivate the channels in the database
                await _channelAccessor.DeactivateMultipleChannelsAsync(channelList);
            }
            catch (DbException dbEx)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, dbEx, "Database error while deactivating channels.");
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, ex, "Unexpected error while deactivating channels.");
            }
        }

        public Task DeleteChannelAsync(ulong channelId)
        {
            throw new NotImplementedException();
        }

        public async Task RegisterChannelsAsync()
        {
            var guilds = _client.Guilds;

            await AddMultipleChannelsAsync(guilds);
            await DeactivateMultipleChannelsAsync(guilds);
        }

        public Task UpdateChannelAsync(ulong channelId, string channelName)
        {
            throw new NotImplementedException();
        }
    }
}
