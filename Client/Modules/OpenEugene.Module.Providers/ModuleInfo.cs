using Oqtane.Models;
using Oqtane.Modules;

namespace OpenEugene.Module.Providers
{
    // No ServerManagerType: the tables come from the Sql project, so the
    // module has no migrations to install.
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Provider",
            Description = "List and manage the providers in the Little Help Book",
            Version = "1.0.0"
        };
    }
}
