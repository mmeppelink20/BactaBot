using Discord;
using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IButtons
    {
        Task BtnDm(SocketMessageComponent component);
        Task BtnShare(SocketMessageComponent component);
        void CreateBtnDM(ComponentBuilder builder, bool isDisabled = false);
        void CreateBtnShare(ComponentBuilder builder, bool isDisabled = false);
    }
}
