using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;
using OpenEugene.LittleHelpBook.Models;

namespace OpenEugene.Module.Categories.Services
{
    public class ClientCategoryService : ServiceBase, ICategoryService
    {
        public ClientCategoryService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Category");

        public async Task<List<CategoryTree>> GetCategoriesAsync(int moduleId)
        {
            return await GetJsonAsync(CreateAuthorizationPolicyUrl(Apiurl, EntityNames.Module, moduleId), new List<CategoryTree>());
        }

        public async Task<Category> GetCategoryAsync(int categoryId, int moduleId)
        {
            return await GetJsonAsync<Category>(CreateAuthorizationPolicyUrl($"{Apiurl}/{categoryId}", EntityNames.Module, moduleId));
        }

        public async Task<Category> AddCategoryAsync(Category category, int moduleId)
        {
            return await PostJsonAsync(CreateAuthorizationPolicyUrl(Apiurl, EntityNames.Module, moduleId), category);
        }

        public async Task<Category> UpdateCategoryAsync(Category category, int moduleId)
        {
            return await PutJsonAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{category.CategoryId}", EntityNames.Module, moduleId), category);
        }

        public async Task DeleteCategoryAsync(int categoryId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{categoryId}", EntityNames.Module, moduleId));
        }

        public async Task<SubCategory> GetSubCategoryAsync(int subCategoryId, int moduleId)
        {
            return await GetJsonAsync<SubCategory>(CreateAuthorizationPolicyUrl($"{Apiurl}/sub/{subCategoryId}", EntityNames.Module, moduleId));
        }

        public async Task<SubCategory> AddSubCategoryAsync(SubCategory subCategory, int moduleId)
        {
            return await PostJsonAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/sub", EntityNames.Module, moduleId), subCategory);
        }

        public async Task<SubCategory> UpdateSubCategoryAsync(SubCategory subCategory, int moduleId)
        {
            return await PutJsonAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/sub/{subCategory.SubCategoryId}", EntityNames.Module, moduleId), subCategory);
        }

        public async Task DeleteSubCategoryAsync(int subCategoryId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/sub/{subCategoryId}", EntityNames.Module, moduleId));
        }
    }
}
