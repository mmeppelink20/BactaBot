namespace LogicLayerInterfaces.Helpers
{
    public interface ICommandPermissionHelper
    {
        bool IsUserDeveloper(ulong userId);
    }
}