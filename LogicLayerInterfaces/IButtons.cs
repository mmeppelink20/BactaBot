using Discord;
using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IButtons
    {
        Task BtnDm(SocketMessageComponent component, string message);
        void CreateBtnDM(ComponentBuilder builder, bool isDisabled = false);
        Task BtnShare(SocketMessageComponent component, string message);
        void CreateBtnShare(ComponentBuilder builder, bool isDisabled = false);

    }
}
