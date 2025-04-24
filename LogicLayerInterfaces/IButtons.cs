using Discord;
using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IButtons
    {
        Task BtnDm(SocketMessageComponent component);
        void CreateBtnDM(ComponentBuilder builder, bool isDisabled = false);
        Task BtnShare(SocketMessageComponent component);
        void CreateBtnShare(ComponentBuilder builder, bool isDisabled = false);

    }
}
