using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IButtonManager
    {
        Task ButtonExecutorAsync(SocketMessageComponent component);
    }
}
