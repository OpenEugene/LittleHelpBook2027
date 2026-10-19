using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Oqtane.Services;
using OpenEugene.Module.Categories.Services;

namespace OpenEugene.Module.Categories.Startup
{
    public class CategoryClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(ICategoryService)))
            {
                services.AddScoped<ICategoryService, ClientCategoryService>();
            }
        }
    }
}
