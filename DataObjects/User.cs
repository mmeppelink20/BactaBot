namespace DataObjects
{
    public class User
    {
        public required ulong UserId { get; set; }
        public string? Username { get; set; }
        public bool IsBot { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class GuildUser : User
    {
        public ulong GuildId { get; set; }
        public string? Nickname { get; set; }
        public DateTime JoinedDate { get; set; }

    }
}
