using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IEventManager
    {
        Task MessageRecieved(SocketMessage message);
        Task SlashCommandExecuted(SocketSlashCommand command);
        Task ButtonExecuted(SocketMessageComponent component);

    }
}
