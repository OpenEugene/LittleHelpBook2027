using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Modules;
using OpenEugene.LittleHelpBook.Models;
using OpenEugene.LittleHelpBook.Repository;

namespace OpenEugene.Module.Providers.Repository
{
    public interface IProviderRepository
    {
        IEnumerable<ProviderListItem> GetProviders();
        ProviderDetail GetProvider(int providerId);
        ProviderDetail AddProvider(ProviderDetail provider);
        ProviderDetail UpdateProvider(ProviderDetail provider);
        void DeleteProvider(int providerId);
        List<CategoryTree> GetCategories();
    }

    // Relations are kept here rather than in the database: child rows are
    // saved and deleted with their provider, and choosing a subcategory also
    // links the provider to that subcategory's parent category.
    public class ProviderRepository : IProviderRepository, ITransientService
    {
        private readonly IDbContextFactory<LittleHelpBookContext> _factory;

        public ProviderRepository(IDbContextFactory<LittleHelpBookContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<ProviderListItem> GetProviders()
        {
            using var db = _factory.CreateDbContext();
            var categoryIds = db.ProviderCategory.AsNoTracking()
                .Select(item => new { item.ProviderId, item.CategoryId })
                .ToList()
                .GroupBy(item => item.ProviderId)
                .ToDictionary(group => group.Key, group => group.Select(item => item.CategoryId).ToList());
            var subCategoryIds = db.ProviderSubCategory.AsNoTracking()
                .Select(item => new { item.ProviderId, item.SubCategoryId })
                .ToList()
                .GroupBy(item => item.ProviderId)
                .ToDictionary(group => group.Key, group => group.Select(item => item.SubCategoryId).ToList());

            return db.Provider.AsNoTracking()
                .OrderBy(item => item.Name)
                .ToList()
                .Select(item => new ProviderListItem
                {
                    ProviderId = item.ProviderId,
                    Name = item.Name,
                    IsActive = item.IsActive,
                    CategoryIds = categoryIds.TryGetValue(item.ProviderId, out var ids) ? ids : new List<int>(),
                    SubCategoryIds = subCategoryIds.TryGetValue(item.ProviderId, out var subIds) ? subIds : new List<int>()
                })
                .ToList();
        }

        public ProviderDetail GetProvider(int providerId)
        {
            using var db = _factory.CreateDbContext();
            var provider = db.Provider.AsNoTracking().FirstOrDefault(item => item.ProviderId == providerId);
            if (provider == null)
            {
                return null;
            }

            return new ProviderDetail
            {
                Provider = provider,
                Addresses = db.Address.AsNoTracking().Where(item => item.ProviderId == providerId).ToList(),
                PhoneNumbers = db.PhoneNumber.AsNoTracking().Where(item => item.ProviderId == providerId).ToList(),
                CategoryIds = db.ProviderCategory.AsNoTracking().Where(item => item.ProviderId == providerId).Select(item => item.CategoryId).ToList(),
                SubCategoryIds = db.ProviderSubCategory.AsNoTracking().Where(item => item.ProviderId == providerId).Select(item => item.SubCategoryId).ToList()
            };
        }

        public ProviderDetail AddProvider(ProviderDetail provider)
        {
            using var db = _factory.CreateDbContext();
            using var transaction = db.Database.BeginTransaction();
            db.Provider.Add(provider.Provider);
            db.SaveChanges();
            SaveChildren(db, provider);
            db.SaveChanges();
            transaction.Commit();
            return GetProvider(provider.Provider.ProviderId);
        }

        public ProviderDetail UpdateProvider(ProviderDetail provider)
        {
            using var db = _factory.CreateDbContext();
            using var transaction = db.Database.BeginTransaction();
            db.Entry(provider.Provider).State = EntityState.Modified;
            SaveChildren(db, provider);
            db.SaveChanges();
            transaction.Commit();
            return GetProvider(provider.Provider.ProviderId);
        }

        public void DeleteProvider(int providerId)
        {
            using var db = _factory.CreateDbContext();
            using var transaction = db.Database.BeginTransaction();
            db.ProviderSubCategory.RemoveRange(db.ProviderSubCategory.Where(item => item.ProviderId == providerId));
            db.ProviderCategory.RemoveRange(db.ProviderCategory.Where(item => item.ProviderId == providerId));
            db.PhoneNumber.RemoveRange(db.PhoneNumber.Where(item => item.ProviderId == providerId));
            db.Address.RemoveRange(db.Address.Where(item => item.ProviderId == providerId));
            var provider = db.Provider.Find(providerId);
            if (provider != null)
            {
                db.Provider.Remove(provider);
            }
            db.SaveChanges();
            transaction.Commit();
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

        private static void SaveChildren(LittleHelpBookContext db, ProviderDetail provider)
        {
            int providerId = provider.Provider.ProviderId;

            // Addresses: remove the ones no longer listed, then add or update the rest.
            var keepAddressIds = provider.Addresses.Where(item => item.AddressId != 0).Select(item => item.AddressId).ToList();
            db.Address.RemoveRange(db.Address.Where(item => item.ProviderId == providerId && !keepAddressIds.Contains(item.AddressId)));
            foreach (var address in provider.Addresses)
            {
                address.ProviderId = providerId;
                if (address.AddressId == 0)
                {
                    db.Address.Add(address);
                }
                else
                {
                    db.Address.Update(address);
                }
            }

            // Phone numbers: same approach.
            var keepPhoneIds = provider.PhoneNumbers.Where(item => item.PhoneNumberId != 0).Select(item => item.PhoneNumberId).ToList();
            db.PhoneNumber.RemoveRange(db.PhoneNumber.Where(item => item.ProviderId == providerId && !keepPhoneIds.Contains(item.PhoneNumberId)));
            foreach (var phone in provider.PhoneNumbers)
            {
                phone.ProviderId = providerId;
                if (phone.PhoneNumberId == 0)
                {
                    db.PhoneNumber.Add(phone);
                }
                else
                {
                    db.PhoneNumber.Update(phone);
                }
            }

            // Subcategory links, then category links including each chosen subcategory's parent.
            var subCategoryIds = provider.SubCategoryIds.Distinct().ToList();
            var parentCategoryIds = db.SubCategory.AsNoTracking()
                .Where(item => subCategoryIds.Contains(item.SubCategoryId))
                .Select(item => item.CategoryId)
                .ToList();
            var categoryIds = provider.CategoryIds.Union(parentCategoryIds).Distinct().ToList();

            db.ProviderSubCategory.RemoveRange(db.ProviderSubCategory.Where(item => item.ProviderId == providerId && !subCategoryIds.Contains(item.SubCategoryId)));
            var existingSubCategoryIds = db.ProviderSubCategory.AsNoTracking().Where(item => item.ProviderId == providerId).Select(item => item.SubCategoryId).ToList();
            foreach (var subCategoryId in subCategoryIds.Except(existingSubCategoryIds))
            {
                db.ProviderSubCategory.Add(new ProviderSubCategory { ProviderId = providerId, SubCategoryId = subCategoryId });
            }

            db.ProviderCategory.RemoveRange(db.ProviderCategory.Where(item => item.ProviderId == providerId && !categoryIds.Contains(item.CategoryId)));
            var existingCategoryIds = db.ProviderCategory.AsNoTracking().Where(item => item.ProviderId == providerId).Select(item => item.CategoryId).ToList();
            foreach (var categoryId in categoryIds.Except(existingCategoryIds))
            {
                db.ProviderCategory.Add(new ProviderCategory { ProviderId = providerId, CategoryId = categoryId });
            }
        }
    }
}
