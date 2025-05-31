using Discord;
using DataObjects;
using static DataObjects.ButtonIdContainer;

namespace LogicLayer
{
    public static class ButtonIdExtensions
    {
        public static string GetLabel(this ButtonId id) => id switch
        {
            ButtonId.btnDm => "DM",
            ButtonId.btnShare => "Share",
            ButtonId.btnRespin => "Respin",
            _ => "Unknown"
        };

        public static ButtonStyle GetStyle(this ButtonId id) => id switch
        {
            ButtonId.btnDm => ButtonStyle.Primary,
            ButtonId.btnShare => ButtonStyle.Success,
            ButtonId.btnRespin => ButtonStyle.Secondary,
            _ => ButtonStyle.Secondary
        };
    }
}
