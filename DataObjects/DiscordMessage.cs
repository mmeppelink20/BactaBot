namespace DataObjects
{
    public class DiscordMessage
    {
        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }
        public string? Content { get; set; }
        public string? CleanContent { get; set; }
        public DateTime MessageDatetime { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? MessageEditedDatetime { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? MessageLink { get; set; }
        public ulong? RepliedToMessageId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? MessageDeletedDatetime { get; set; }

    }

    public class DiscordMessageVM : DiscordMessage
    {
        public string? UserName { get; set; }
        public DateTime? JoinedDate { get; set; }
        public string? NickName { get; set; }
        public string? ChannelName { get; set; }
        public string? ChannelType { get; set; }
        public ulong? GuildId { get; set; }

        public string ToStringForCompletion()
        {
            var repliedToMessageId = RepliedToMessageId != null ? $"[REPLIED TO MESSAGE ID: {RepliedToMessageId}] " : string.Empty;

            return $"[USERNAME/NICKNAME: {UserName} / {NickName}] [MESSAGE ID: {MessageId}] [MESSAGE: {CleanContent}] [TIMESTAMP: {MessageDatetime}] {repliedToMessageId}";
        }

        public string ToStringForDeletedMessage()
        {
            var repliedToMessageId = RepliedToMessageId != null ? $"[REPLIED TO MESSAGE ID: {RepliedToMessageId}] " : string.Empty;

            return $"[USERNAME/NICKNAME: {UserName} / {NickName}] [THIS MESSAGE WAS DELETED] [TIMESTAMP: {MessageDatetime}] [MESSAGE WAS DELETED AT: {MessageDeletedDatetime}] {repliedToMessageId}";
        }

    }

}
