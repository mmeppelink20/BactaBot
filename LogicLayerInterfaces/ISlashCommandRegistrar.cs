using DataObjects;
using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface ISlashCommandRegistrar
    {
        Task<OperationResult> RegisterCommandsAsync();
        Task<OperationResult> DeleteCommandAsync(SocketApplicationCommand command);
        Task<OperationResult<IEnumerable<SocketApplicationCommand>>> GetRegisteredSlashCommandsAsync();

        // Legacy methods for backward compatibility
        Task DeleteCommand(SocketApplicationCommand command);
    }
}