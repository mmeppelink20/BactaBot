using Discord;
using Discord.WebSocket;
using LogicLayerInterfaces;
using LogicLayerInterfaces.CommandHandlers;
using Microsoft.Extensions.Logging;
using DataObjects;

namespace LogicLayer.CommandHandlers
{
    public class DeveloperCommandHandler : IDeveloperCommandHandler
    {
        private readonly ILogger<IDeveloperCommandHandler> _logger;
        private readonly DiscordSocketClient _client;
        private readonly IBactaConfigurationManager _bactaConfigurationManager;

        public DeveloperCommandHandler(
            ILogger<IDeveloperCommandHandler> logger,
            DiscordSocketClient client,
            IBactaConfigurationManager bactaConfigurationManager)
        {
            _logger = logger;
            _client = client;
            _bactaConfigurationManager = bactaConfigurationManager;
        }

        public async Task HandleAddCommand(SocketSlashCommand command, SocketSlashCommandDataOption subCommand)
        {
            var userOption = subCommand.Options.FirstOrDefault(x => x.Name == "user");

            if (userOption?.Value == null)
            {
                await command.FollowupAsync("❌ **Error:** User is required.", ephemeral: true);
                return;
            }

            if (userOption.Value is not SocketUser targetUser)
            {
                await command.FollowupAsync("❌ **Error:** Invalid user specified.", ephemeral: true);
                return;
            }

            var userId = targetUser.Id;

            var currentListResult = await _bactaConfigurationManager.RetrieveConfigurationKeyValueAsync(ConfigurationKeys.DeveloperUserIdList);

            if (currentListResult.IsFailure)
            {
                await command.FollowupAsync($"❌ **Error retrieving developer list:** {currentListResult.ErrorMessage}", ephemeral: true);
                return;
            }

            var currentList = currentListResult.Data?.Value ?? string.Empty;
            var developerIds = string.IsNullOrEmpty(currentList)
                ? new HashSet<ulong>()
                : new HashSet<ulong>(currentList.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse));

            if (developerIds.Contains(userId))
            {
                await command.FollowupAsync($"⚠️ **Warning:** {targetUser.Mention} is already in the developer list.", ephemeral: true);
                return;
            }

            developerIds.Add(userId);
            var newList = string.Join(",", developerIds);

            var result = await _bactaConfigurationManager.SetConfigurationKeyValueAsync(ConfigurationKeys.DeveloperUserIdList, newList);

            if (result.IsSuccess)
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle("✅ Developer Added")
                    .AddField("User", targetUser.Mention, true)
                    .AddField("User ID", userId.ToString(), true)
                    .AddField("Total Developers", developerIds.Count.ToString(), true)
                    .AddField("Status", "Successfully added to developer list", false)
                    .WithFooter($"Command executed by {command.User.Username}")
                    .WithTimestamp(DateTimeOffset.UtcNow);

                await command.FollowupAsync(embed: embed.Build(), ephemeral: true);

                _logger.LogCritical("User {TargetUser} ({TargetUserId}) added to developer list by {User} ({UserId})",
                    targetUser.Username, userId, command.User.Username, command.User.Id);
            }
            else
            {
                await command.FollowupAsync($"❌ **Error adding developer:** {result.ErrorMessage}", ephemeral: true);
            }
        }

        public async Task HandleRemoveCommand(SocketSlashCommand command, SocketSlashCommandDataOption subCommand)
        {
            var userOption = subCommand.Options.FirstOrDefault(x => x.Name == "user");

            if (userOption?.Value == null)
            {
                await command.FollowupAsync("❌ **Error:** User is required.", ephemeral: true);
                return;
            }

            if (userOption.Value is not SocketUser targetUser)
            {
                await command.FollowupAsync("❌ **Error:** Invalid user specified.", ephemeral: true);
                return;
            }

            var userId = targetUser.Id;

            var listResult = await _bactaConfigurationManager.RetrieveConfigurationKeyValueAsync(ConfigurationKeys.DeveloperUserIdList);

            if (listResult.IsFailure)
            {
                await command.FollowupAsync($"❌ **Error retrieving developer list:** {listResult.ErrorMessage}", ephemeral: true);
                return;
            }

            var currentList = listResult.Data?.Value ?? string.Empty;
            var developerIds = string.IsNullOrEmpty(currentList)
                ? new HashSet<ulong>()
                : new HashSet<ulong>(currentList.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse));

            if (userId == command.User.Id && developerIds.Count <= 1)
            {
                await command.FollowupAsync("❌ **Error:** Cannot remove yourself as the only developer.", ephemeral: true);
                return;
            }

            if (!developerIds.Contains(userId))
            {
                await command.FollowupAsync($"⚠️ **Warning:** {targetUser.Mention} is not in the developer list.", ephemeral: true);
                return;
            }

            developerIds.Remove(userId);
            var newList = string.Join(",", developerIds);

            var result = await _bactaConfigurationManager.SetConfigurationKeyValueAsync(ConfigurationKeys.DeveloperUserIdList, newList);

            if (result.IsSuccess)
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithTitle("🗑️ Developer Removed")
                    .AddField("User", targetUser.Mention, true)
                    .AddField("User ID", userId.ToString(), true)
                    .AddField("Remaining Developers", developerIds.Count.ToString(), true)
                    .AddField("Status", "Successfully removed from developer list", false)
                    .WithFooter($"Command executed by {command.User.Username}")
                    .WithTimestamp(DateTimeOffset.UtcNow);

                await command.FollowupAsync(embed: embed.Build(), ephemeral: true);

                _logger.LogCritical("User {TargetUser} ({TargetUserId}) removed from developer list by {User} ({UserId})",
                    targetUser.Username, userId, command.User.Username, command.User.Id);
            }
            else
            {
                await command.FollowupAsync($"❌ **Error removing developer:** {result.ErrorMessage}", ephemeral: true);
            }
        }

        public async Task HandleListCommand(SocketSlashCommand command)
        {
            var result = await _bactaConfigurationManager.RetrieveConfigurationKeyValueAsync(ConfigurationKeys.DeveloperUserIdList);

            if (result.IsFailure)
            {
                await command.FollowupAsync($"❌ **Error retrieving developer list:** {result.ErrorMessage}", ephemeral: true);
                return;
            }

            var currentList = result.Data?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(currentList))
            {
                await command.FollowupAsync("📋 **No developers found in the list.**", ephemeral: true);
                return;
            }

            var developerIds = currentList.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse);
            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle("👨‍💻 Developer List")
                .WithDescription($"Found {developerIds.Count()} developers:")
                .WithFooter($"Command executed by {command.User.Username}")
                .WithTimestamp(DateTimeOffset.UtcNow);

            var developerInfo = new List<string>();
            foreach (var id in developerIds)
            {
                var user = _client.GetUser(id);
                var userInfo = user != null ? $"<@{id}> (`{user.Username}`)" : $"<@{id}> (Unknown User)";
                developerInfo.Add($"• {userInfo}");
            }

            embed.AddField("Developers", string.Join("\n", developerInfo), false);

            await command.FollowupAsync(embed: embed.Build(), ephemeral: true);

            _logger.LogCritical("Developer list requested by {User} ({UserId})", command.User.Username, command.User.Id);
        }
    }
}