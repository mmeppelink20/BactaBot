namespace LogicLayerInterfaces.CommandHandlers
{
    public interface ICommandPermissionHelper
    {
        bool IsUserDeveloper(ulong userId);
    }
}