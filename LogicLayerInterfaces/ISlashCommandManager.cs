using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface ISlashCommandManager
    {
        public Task CommandExecutorAsync(SocketSlashCommand command);

    }
}
