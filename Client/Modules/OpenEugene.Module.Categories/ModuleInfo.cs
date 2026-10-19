using Oqtane.Models;
using Oqtane.Modules;

namespace OpenEugene.Module.Categories
{
    // No ServerManagerType: the tables come from the Sql project, so the
    // module has no migrations to install.
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Category",
            Description = "Manage the categories and subcategories in the Little Help Book",
            Version = "1.0.0"
        };
    }
}
