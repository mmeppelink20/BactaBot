using Microsoft.Extensions.DependencyInjection;

namespace LogicLayerInterfaces
{
    public interface IBot
    {
        Task StartAsync(ServiceProvider services);
        Task StopAsync();
    }
}