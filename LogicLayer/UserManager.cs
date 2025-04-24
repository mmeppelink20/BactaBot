using DataAccessLayerInterfaces;
using DataObjects;
using Discord;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace LogicLayer
{
    public class UserManager(ILogger<IUserManager> logger, IConfiguration configuration, IGuildAccessor guildAccessor, IUserAccessor userAccessor, DiscordSocketClient client) : IUserManager
    {
        private readonly ILogger<IUserManager> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IGuildAccessor _guildAccessor = guildAccessor;
        private readonly IUserAccessor _userAccessor = userAccessor;
        private readonly DiscordSocketClient _client = client;

        public async Task AddMultipleUsersAsync(Dictionary<ulong, User> users)
        {
            try
            {
                await _userAccessor.InsertMultipleUsersAsync(users);

                _logger.LogInformation((int)BactaLogging.LogEvent.StartUpShutDown, "Users added to the database.");
            }
            catch (DbException dbEx)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, dbEx, "Database error while adding users.");
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, ex, "Failed to add users to the database.");
            }
        }

        public Task AddUserAsync(SocketUser user)
        {
            throw new NotImplementedException();
        }

        public async Task DeactivateMultipleUsersAsync(Dictionary<ulong, User> users)
        {
            try
            {
                await _userAccessor.DeactivateMultipleUsersAsync(users);

                _logger.LogInformation((int)BactaLogging.LogEvent.StartUpShutDown, "Users deactivated in the database.");
            }
            catch (DbException dbEx)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, dbEx, "Database error while deactivating users.");
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, ex, "Failed to deactivate users in the database.");
            }
        }

        public async Task RegisterUsersAsync()
        {
            Dictionary<ulong, User> users = [];

            // download all the users all the guilds
            var guilds = _client.Guilds;

            // add users to the list, check userid for key
            foreach (var guild in guilds)
            {
                var guildUsers = guild.Users;
                foreach (var user in guildUsers)
                {
                    if (!users.ContainsKey(user.Id))
                    {
                        users.Add(user.Id, new User
                        {
                            UserId = user.Id,
                            Username = user.Username,
                            IsBot = user.IsBot,
                            AvatarUrl = user.GetAvatarUrl(ImageFormat.Auto, 256)
                        });
                    }
                }
            }

            await AddMultipleUsersAsync(users);
        }
        public async Task RegisterGuildUsersAsync()
        {
            Dictionary<ulong, List<GuildUser>> users = [];

            // download all the users all the guilds
            var guilds = _client.Guilds;

            foreach (var guild in guilds)
            {
                var guildUsers = guild.Users;
                foreach (var user in guildUsers)
                {
                    if (!users.ContainsKey(guild.Id))
                    {
                        users.Add(guild.Id, new List<GuildUser>());
                    }
                    users[guild.Id].Add(new GuildUser
                    {
                        GuildId = guild.Id,
                        UserId = user.Id,
                        Nickname = user.Nickname,
                        JoinedDate = user.JoinedAt.HasValue ? user.JoinedAt.Value.DateTime : DateTime.MinValue
                    });
                }

            }

            await AddGuildUsersAsync(users);
            await DeactivateGuildUsersAsync(users);
        }

        public async Task AddGuildUsersAsync(Dictionary<ulong, List<GuildUser>> guildUsers)
        {
            try
            {
                await _userAccessor.InsertGuildUsers(guildUsers);
                _logger.LogInformation((int)BactaLogging.LogEvent.StartUpShutDown, "Guild users added to the database.");
            }
            catch (DbException dbEx)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, dbEx, "Database error while adding guild users.");
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, ex, "Failed to add guild users to the database.");
            }
        }

        public async Task DeactivateGuildUsersAsync(Dictionary<ulong, List<GuildUser>> guildUsers)
        {
            try
            {
                await _userAccessor.DeactivateGuildUsers(guildUsers);
                _logger.LogInformation((int)BactaLogging.LogEvent.StartUpShutDown, "Guild users deactivated in the database.");
            }
            catch (DbException dbEx)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, dbEx, "Database error while deactivating guild users.");
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, ex, "Failed to deactivate guild users in the database.");
            }
        }

    }
}
