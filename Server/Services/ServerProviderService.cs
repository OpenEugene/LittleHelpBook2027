using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenEugene.LittleHelpBook.Models;
using OpenEugene.Module.Providers.Repository;

namespace OpenEugene.Module.Providers.Services
{
    // Authorization is handled by the controller's module policies.
    public class ServerProviderService : IProviderService
    {
        private readonly IProviderRepository _providerRepository;

        public ServerProviderService(IProviderRepository providerRepository)
        {
            _providerRepository = providerRepository;
        }

        public Task<List<ProviderListItem>> GetProvidersAsync(int moduleId)
        {
            return Task.FromResult(_providerRepository.GetProviders().ToList());
        }

        public Task<ProviderDetail> GetProviderAsync(int providerId, int moduleId)
        {
            return Task.FromResult(_providerRepository.GetProvider(providerId));
        }

        public Task<ProviderDetail> AddProviderAsync(ProviderDetail provider, int moduleId)
        {
            return Task.FromResult(_providerRepository.AddProvider(provider));
        }

        public Task<ProviderDetail> UpdateProviderAsync(ProviderDetail provider, int moduleId)
        {
            return Task.FromResult(_providerRepository.UpdateProvider(provider));
        }

        public Task DeleteProviderAsync(int providerId, int moduleId)
        {
            _providerRepository.DeleteProvider(providerId);
            return Task.CompletedTask;
        }

        public Task<List<CategoryTree>> GetCategoriesAsync(int moduleId)
        {
            return Task.FromResult(_providerRepository.GetCategories());
        }
    }
}
