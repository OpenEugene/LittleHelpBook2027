using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Repository.Databases.Interfaces;
using OpenEugene.LittleHelpBook.Models;

namespace OpenEugene.LittleHelpBook.Repository
{
    // One context for the book's tables. The tables are defined in the Sql
    // project, so there are no migrations and no relationship configuration.
    public class LittleHelpBookContext : DBContextBase, ITransientService, IMultiDatabase
    {
        public virtual DbSet<Provider> Provider { get; set; }
        public virtual DbSet<Address> Address { get; set; }
        public virtual DbSet<PhoneNumber> PhoneNumber { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<SubCategory> SubCategory { get; set; }
        public virtual DbSet<ProviderCategory> ProviderCategory { get; set; }
        public virtual DbSet<ProviderSubCategory> ProviderSubCategory { get; set; }

        public LittleHelpBookContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
            // ContextBase handles multi-tenant database connections
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Provider>().ToTable(ActiveDatabase.RewriteName("Provider"));
            builder.Entity<Address>().ToTable(ActiveDatabase.RewriteName("Address"));
            builder.Entity<PhoneNumber>().ToTable(ActiveDatabase.RewriteName("PhoneNumber"));
            builder.Entity<Category>().ToTable(ActiveDatabase.RewriteName("Category"));
            builder.Entity<SubCategory>().ToTable(ActiveDatabase.RewriteName("SubCategory"));
            builder.Entity<ProviderCategory>().ToTable(ActiveDatabase.RewriteName("ProviderCategory"));
            builder.Entity<ProviderSubCategory>().ToTable(ActiveDatabase.RewriteName("ProviderSubCategory"));
        }
    }
}
