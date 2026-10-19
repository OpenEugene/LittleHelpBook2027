using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Modules;
using OpenEugene.LittleHelpBook.Models;
using OpenEugene.LittleHelpBook.Repository;

namespace OpenEugene.Module.Categories.Repository
{
    public interface ICategoryRepository
    {
        List<CategoryTree> GetCategories();
        Category GetCategory(int categoryId);
        Category AddCategory(Category category);
        Category UpdateCategory(Category category);
        void DeleteCategory(int categoryId);
        SubCategory GetSubCategory(int subCategoryId);
        SubCategory AddSubCategory(SubCategory subCategory);
        SubCategory UpdateSubCategory(SubCategory subCategory);
        void DeleteSubCategory(int subCategoryId);
    }

    // Relations are kept here rather than in the database: deleting a
    // category removes its subcategories and every provider link to them.
    public class CategoryRepository : ICategoryRepository, ITransientService
    {
        private readonly IDbContextFactory<LittleHelpBookContext> _factory;

        public CategoryRepository(IDbContextFactory<LittleHelpBookContext> factory)
        {
            _factory = factory;
        }

        public List<CategoryTree> GetCategories()
        {
            using var db = _factory.CreateDbContext();
            var subCategories = db.SubCategory.AsNoTracking().OrderBy(item => item.Name).ToList();
            return db.Category.AsNoTracking()
                .OrderBy(item => item.Name)
                .ToList()
                .Select(category => new CategoryTree
                {
                    Category = category,
                    SubCategories = subCategories.Where(item => item.CategoryId == category.CategoryId).ToList()
                })
                .ToList();
        }

        public Category GetCategory(int categoryId)
        {
            using var db = _factory.CreateDbContext();
            return db.Category.AsNoTracking().FirstOrDefault(item => item.CategoryId == categoryId);
        }

        public Category AddCategory(Category category)
        {
            using var db = _factory.CreateDbContext();
            db.Category.Add(category);
            db.SaveChanges();
            return category;
        }

        public Category UpdateCategory(Category category)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(category).State = EntityState.Modified;
            db.SaveChanges();
            return category;
        }

        public void DeleteCategory(int categoryId)
        {
            using var db = _factory.CreateDbContext();
            using var transaction = db.Database.BeginTransaction();
            var subCategoryIds = db.SubCategory.Where(item => item.CategoryId == categoryId).Select(item => item.SubCategoryId).ToList();
            db.ProviderSubCategory.RemoveRange(db.ProviderSubCategory.Where(item => subCategoryIds.Contains(item.SubCategoryId)));
            db.SubCategory.RemoveRange(db.SubCategory.Where(item => item.CategoryId == categoryId));
            db.ProviderCategory.RemoveRange(db.ProviderCategory.Where(item => item.CategoryId == categoryId));
            var category = db.Category.Find(categoryId);
            if (category != null)
            {
                db.Category.Remove(category);
            }
            db.SaveChanges();
            transaction.Commit();
        }

        public SubCategory GetSubCategory(int subCategoryId)
        {
            using var db = _factory.CreateDbContext();
            return db.SubCategory.AsNoTracking().FirstOrDefault(item => item.SubCategoryId == subCategoryId);
        }

        public SubCategory AddSubCategory(SubCategory subCategory)
        {
            using var db = _factory.CreateDbContext();
            db.SubCategory.Add(subCategory);
            db.SaveChanges();
            return subCategory;
        }

        public SubCategory UpdateSubCategory(SubCategory subCategory)
        {
            using var db = _factory.CreateDbContext();
            using var transaction = db.Database.BeginTransaction();
            db.Entry(subCategory).State = EntityState.Modified;

            // If the subcategory moved to another category, providers in it
            // must also be in the new parent category.
            var providerIds = db.ProviderSubCategory.AsNoTracking()
                .Where(item => item.SubCategoryId == subCategory.SubCategoryId)
                .Select(item => item.ProviderId)
                .ToList();
            var alreadyLinked = db.ProviderCategory.AsNoTracking()
                .Where(item => item.CategoryId == subCategory.CategoryId && providerIds.Contains(item.ProviderId))
                .Select(item => item.ProviderId)
                .ToList();
            foreach (var providerId in providerIds.Except(alreadyLinked))
            {
                db.ProviderCategory.Add(new ProviderCategory { ProviderId = providerId, CategoryId = subCategory.CategoryId });
            }

            db.SaveChanges();
            transaction.Commit();
            return subCategory;
        }

        public void DeleteSubCategory(int subCategoryId)
        {
            using var db = _factory.CreateDbContext();
            using var transaction = db.Database.BeginTransaction();
            db.ProviderSubCategory.RemoveRange(db.ProviderSubCategory.Where(item => item.SubCategoryId == subCategoryId));
            var subCategory = db.SubCategory.Find(subCategoryId);
            if (subCategory != null)
            {
                db.SubCategory.Remove(subCategory);
            }
            db.SaveChanges();
            transaction.Commit();
        }
    }
}
