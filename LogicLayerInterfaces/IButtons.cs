using Discord;
using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IButtons
    {
        Task BtnDm(SocketMessageComponent component, string? message);
        Task BtnShare(SocketMessageComponent component, string? message);
        Task BtnRespin(SocketMessageComponent component);
        void CreateBtnDM(ComponentBuilder builder, bool isDisabled = false);
        void CreateBtnShare(ComponentBuilder builder, bool isDisabled = false);
        void CreateBtnRespin(ComponentBuilder builder, bool isDisabled = false);
    }
}
