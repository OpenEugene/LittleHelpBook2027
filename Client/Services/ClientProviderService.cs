using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;
using OpenEugene.LittleHelpBook.Models;

namespace OpenEugene.Module.Providers.Services
{
    public class ClientProviderService : ServiceBase, IProviderService
    {
        public ClientProviderService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Provider");

        public async Task<List<ProviderListItem>> GetProvidersAsync(int moduleId)
        {
            return await GetJsonAsync(CreateAuthorizationPolicyUrl(Apiurl, EntityNames.Module, moduleId), new List<ProviderListItem>());
        }

        public async Task<ProviderDetail> GetProviderAsync(int providerId, int moduleId)
        {
            return await GetJsonAsync<ProviderDetail>(CreateAuthorizationPolicyUrl($"{Apiurl}/{providerId}", EntityNames.Module, moduleId));
        }

        public async Task<ProviderDetail> AddProviderAsync(ProviderDetail provider, int moduleId)
        {
            return await PostJsonAsync(CreateAuthorizationPolicyUrl(Apiurl, EntityNames.Module, moduleId), provider);
        }

        public async Task<ProviderDetail> UpdateProviderAsync(ProviderDetail provider, int moduleId)
        {
            return await PutJsonAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{provider.Provider.ProviderId}", EntityNames.Module, moduleId), provider);
        }

        public async Task DeleteProviderAsync(int providerId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{providerId}", EntityNames.Module, moduleId));
        }

        public async Task<List<CategoryTree>> GetCategoriesAsync(int moduleId)
        {
            return await GetJsonAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/categories", EntityNames.Module, moduleId), new List<CategoryTree>());
        }
    }
}
