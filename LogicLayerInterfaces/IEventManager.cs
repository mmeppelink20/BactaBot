using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IEventManager
    {
        Func<Task>? ShutdownRequested { get; set; }
        Task MessageRecieved(SocketMessage message);
        Task SlashCommandExecuted(SocketSlashCommand command);
        Task ButtonExecuted(SocketMessageComponent component);

    }
}
