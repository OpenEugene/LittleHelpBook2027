using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using MudBlazor;
using MudBlazor.Services;
using Oqtane.Services;

namespace OpenEugene.LittleHelpBook.Startup
{
    // Registers MudBlazor once for all LittleHelpBook modules. Oqtane also runs
    // client startups on the server, so this covers server rendering too.
    public class MudBlazorClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(IDialogService)))
            {
                services.AddMudServices();
            }
        }
    }
}
