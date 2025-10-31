using DataAccessLayerInterfaces;
using DataObjects;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogicLayer
{
    public class GuildMessageManager(ILogger<IGuildMessageManager> logger, IConfiguration configuration, DiscordSocketClient client, CommandService commands, IDiscordMessageAccessor discordMessageAccessor) : IGuildMessageManager
    {
        private readonly ILogger<IGuildMessageManager> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly DiscordSocketClient _client = client;
        private readonly CommandService _commands = commands;
        private readonly IDiscordMessageAccessor _discordMessageAccessor = discordMessageAccessor;

        public async Task AddDiscordMessageAsync(SocketMessage message)
        {
            try
            {
                if (message.Channel is IGuildChannel guildChannel)
                {
                    DiscordMessageVM discordMessage = new()
                    {
                        MessageId = message.Id,
                        ChannelId = message.Channel.Id,
                        UserId = message.Author.Id,
                        UserName = message.Author.Username,
                        NickName = (message.Author as SocketGuildUser)?.Nickname,
                        JoinedDate = (message.Author as SocketGuildUser)?.JoinedAt?.DateTime,
                        Content = message.Content,
                        CleanContent = message.CleanContent,
                        MessageDatetime = message.Timestamp.DateTime,
                        IsEdited = message.EditedTimestamp.HasValue,
                        MessageEditedDatetime = message.EditedTimestamp?.DateTime,
                        AttachmentUrl = message.Attachments.Count > 0 ? message.Attachments.First().Url : null,
                        MessageLink = message.GetJumpUrl(),
                        RepliedToMessageId = (ulong?)(message.Reference?.MessageId),
                        ChannelName = guildChannel.Name,
                        ChannelType = guildChannel.ChannelType.ToString(),
                        GuildId = guildChannel.GuildId,
                        AvatarUrl = (message.Author as SocketGuildUser)?.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl(),
                    };

                    await _discordMessageAccessor.InsertDiscordMessage(discordMessage);
                    _logger.LogTrace((int)BactaLogging.LogEvent.MessageRelated, "Message added to database: {message.Content}", message.Content);
                }
                else
                {
                    _logger.LogWarning("Message is not from a guild channel: {message.Content}", message.Content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.AddGuildMessage, ex, "An error occurred while adding the guild message");
            }
        }

        public async Task AddDiscordMessagesAsync(IEnumerable<SocketMessage> messages)
        {
            var discordMessages = new List<DiscordMessageVM>();

            foreach (var message in messages)
            {
                if (message.Channel is IGuildChannel guildChannel)
                {
                    discordMessages.Add(new DiscordMessageVM
                    {
                        MessageId = message.Id,
                        ChannelId = message.Channel.Id,
                        UserId = message.Author.Id,
                        UserName = message.Author.Username,
                        NickName = (message.Author as SocketGuildUser)?.Nickname,
                        JoinedDate = (message.Author as SocketGuildUser)?.JoinedAt?.DateTime,
                        Content = message.Content,
                        CleanContent = message.CleanContent,
                        MessageDatetime = message.Timestamp.DateTime,
                        IsEdited = message.EditedTimestamp.HasValue,
                        MessageEditedDatetime = message.EditedTimestamp?.DateTime,
                        AttachmentUrl = message.Attachments.Count > 0 ? message.Attachments.First().Url : null,
                        MessageLink = message.GetJumpUrl(),
                        RepliedToMessageId = (ulong?)(message.Reference?.MessageId),
                        ChannelName = guildChannel.Name,
                        ChannelType = guildChannel.ChannelType.ToString(),
                        GuildId = guildChannel.GuildId,
                        AvatarUrl = (message.Author as SocketGuildUser)?.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl(),
                    });
                }
            }

            if (discordMessages.Count > 0)
            {
                foreach(var msg in discordMessages)
                {
                    _logger.LogDebug("Prepared message for bulk insert: {MessageId} in Channel {ChannelId} by User {UserId}", msg.MessageId, msg.ChannelId, msg.UserId);
                    await _discordMessageAccessor.InsertDiscordMessage(msg);
                }
                _logger.LogInformation("Bulk inserted {Count} messages.", discordMessages.Count);
            }
        }

        public async Task AddDiscordMessagesAsync(IEnumerable<IMessage> messages)
        {
            var discordMessages = new List<DiscordMessageVM>();

            foreach (var message in messages)
            {
                if (message.Channel is IGuildChannel guildChannel)
                {
                    discordMessages.Add(new DiscordMessageVM
                    {
                        MessageId = message.Id,
                        ChannelId = message.Channel.Id,
                        UserId = message.Author.Id,
                        UserName = message.Author.Username,
                        NickName = (message.Author as SocketGuildUser)?.Nickname,
                        JoinedDate = (message.Author as SocketGuildUser)?.JoinedAt?.DateTime,
                        Content = message.Content,
                        CleanContent = message.CleanContent,
                        MessageDatetime = message.Timestamp.DateTime,
                        IsEdited = message.EditedTimestamp.HasValue,
                        MessageEditedDatetime = message.EditedTimestamp?.DateTime,
                        AttachmentUrl = message.Attachments.Count > 0 ? message.Attachments.First().Url : null,
                        MessageLink = message.GetJumpUrl(),
                        RepliedToMessageId = (ulong?)(message.Reference?.MessageId),
                        ChannelName = guildChannel.Name,
                        ChannelType = guildChannel.ChannelType.ToString(),
                        GuildId = guildChannel.GuildId,
                        AvatarUrl = (message.Author as SocketGuildUser)?.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl(),
                    });
                }
            }

            if (discordMessages.Count > 0)
            {
                foreach (var msg in discordMessages)
                {
                    _logger.LogDebug("Prepared message for bulk insert: {MessageId} in Channel {ChannelId} by User {UserId}", msg.MessageId, msg.ChannelId, msg.UserId);
                    await _discordMessageAccessor.InsertDiscordMessage(msg);
                }
                _logger.LogInformation("Bulk inserted {Count} messages.", discordMessages.Count);
            }
        }

        public async Task<bool> DeleteDiscordMessageAsync(ulong messageID)
        {
            bool isDeleted = false;

            try
            {
                isDeleted = await _discordMessageAccessor.DeleteDiscordMessage(messageID); 
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.MessageRelated, ex, "An error occurred while deleting the guild message");

            }

            return isDeleted;
        }

        public Task<List<DiscordMessageVM>> RetrieveDiscordMessagesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<DiscordMessageVM>> RetrieveDiscordMessagesByChannelIDAndMinutesAsync(ulong channelID, int minutes)
        {
            List<DiscordMessageVM> messages = new List<DiscordMessageVM>();

            try
            {
                messages = await _discordMessageAccessor.GetDiscordMessagesByChannelIDAndMinutesAsync(channelID, minutes);
                _logger.LogInformation((int)BactaLogging.LogEvent.MessageRelated, "Retrieved {count} messages from channel {channelID} in the last {minutes} minutes", messages.Count, channelID, minutes);
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.MessageRelated, ex, "An error occurred while retrieving the guild messages");
            }

            return messages;
        }

    }
}
