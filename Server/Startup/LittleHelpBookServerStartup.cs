using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using OpenEugene.LittleHelpBook.Repository;

namespace OpenEugene.LittleHelpBook.Startup
{
    // Registers the shared context once for the Provider and Category modules.
    public class LittleHelpBookServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // not implemented
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // not implemented
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<LittleHelpBookContext>(opt => { }, ServiceLifetime.Transient);
        }
    }
}
