using System.Collections.Generic;
using System.Threading.Tasks;
using OpenEugene.LittleHelpBook.Models;
using OpenEugene.Module.Categories.Repository;

namespace OpenEugene.Module.Categories.Services
{
    // Authorization is handled by the controller's module policies.
    public class ServerCategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public ServerCategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public Task<List<CategoryTree>> GetCategoriesAsync(int moduleId)
        {
            return Task.FromResult(_categoryRepository.GetCategories());
        }

        public Task<Category> GetCategoryAsync(int categoryId, int moduleId)
        {
            return Task.FromResult(_categoryRepository.GetCategory(categoryId));
        }

        public Task<Category> AddCategoryAsync(Category category, int moduleId)
        {
            return Task.FromResult(_categoryRepository.AddCategory(category));
        }

        public Task<Category> UpdateCategoryAsync(Category category, int moduleId)
        {
            return Task.FromResult(_categoryRepository.UpdateCategory(category));
        }

        public Task DeleteCategoryAsync(int categoryId, int moduleId)
        {
            _categoryRepository.DeleteCategory(categoryId);
            return Task.CompletedTask;
        }

        public Task<SubCategory> GetSubCategoryAsync(int subCategoryId, int moduleId)
        {
            return Task.FromResult(_categoryRepository.GetSubCategory(subCategoryId));
        }

        public Task<SubCategory> AddSubCategoryAsync(SubCategory subCategory, int moduleId)
        {
            return Task.FromResult(_categoryRepository.AddSubCategory(subCategory));
        }

        public Task<SubCategory> UpdateSubCategoryAsync(SubCategory subCategory, int moduleId)
        {
            return Task.FromResult(_categoryRepository.UpdateSubCategory(subCategory));
        }

        public Task DeleteSubCategoryAsync(int subCategoryId, int moduleId)
        {
            _categoryRepository.DeleteSubCategory(subCategoryId);
            return Task.CompletedTask;
        }
    }
}
