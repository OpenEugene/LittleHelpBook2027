using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Oqtane.Services;
using OpenEugene.Module.Providers.Services;

namespace OpenEugene.Module.Providers.Startup
{
    public class ProviderClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(IProviderService)))
            {
                services.AddScoped<IProviderService, ClientProviderService>();
            }
        }
    }
}
