using System.Collections.Generic;
using System.Threading.Tasks;
using OpenEugene.LittleHelpBook.Models;

namespace OpenEugene.Module.Categories.Services
{
    // moduleId identifies the calling module instance for authorization;
    // category data itself is shared across the site.
    public interface ICategoryService
    {
        Task<List<CategoryTree>> GetCategoriesAsync(int moduleId);

        Task<Category> GetCategoryAsync(int categoryId, int moduleId);

        Task<Category> AddCategoryAsync(Category category, int moduleId);

        Task<Category> UpdateCategoryAsync(Category category, int moduleId);

        Task DeleteCategoryAsync(int categoryId, int moduleId);

        Task<SubCategory> GetSubCategoryAsync(int subCategoryId, int moduleId);

        Task<SubCategory> AddSubCategoryAsync(SubCategory subCategory, int moduleId);

        Task<SubCategory> UpdateSubCategoryAsync(SubCategory subCategory, int moduleId);

        Task DeleteSubCategoryAsync(int subCategoryId, int moduleId);
    }
}
