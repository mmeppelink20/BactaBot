using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface ISlashCommandRegistrar
    {
        public Task RegisterCommandsAsync();
        public Task DeleteCommand(SocketApplicationCommand command);
        public Task<IEnumerable<SocketApplicationCommand>> GetRegisteredSlashCommandsAsync();
    }
}
