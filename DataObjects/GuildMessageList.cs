using Discord.WebSocket;


namespace DataObjects
{
    public class GuildMessageList
    {
        public required Dictionary<ulong, Dictionary<ulong, List<SocketMessage>>> GuildMessages { get; set; }
        public required Dictionary<string, int> CharacterLimits { get; set; }
    }
}
