using System.Collections.Generic;
using System.Threading.Tasks;
using OpenEugene.LittleHelpBook.Models;

namespace OpenEugene.Module.Providers.Services
{
    // moduleId identifies the calling module instance for authorization;
    // provider data itself is shared across the site.
    public interface IProviderService
    {
        Task<List<ProviderListItem>> GetProvidersAsync(int moduleId);

        Task<ProviderDetail> GetProviderAsync(int providerId, int moduleId);

        Task<ProviderDetail> AddProviderAsync(ProviderDetail provider, int moduleId);

        Task<ProviderDetail> UpdateProviderAsync(ProviderDetail provider, int moduleId);

        Task DeleteProviderAsync(int providerId, int moduleId);

        Task<List<CategoryTree>> GetCategoriesAsync(int moduleId);
    }
}
