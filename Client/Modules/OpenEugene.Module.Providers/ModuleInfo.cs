using System.Collections.Generic;
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
            Version = "1.0.0",
            SettingsType = "OpenEugene.Module.Providers.Settings, OpenEugene.LittleHelpBook.Client.Oqtane",
            Resources = new List<Resource>
            {
                new Stylesheet("https://api.mapbox.com/mapbox-gl-js/v3.33.0/mapbox-gl.css"),
                new Stylesheet("~/Module.css?v=16"),
                new Script("https://api.mapbox.com/mapbox-gl-js/v3.33.0/mapbox-gl.js"),
                new Script("~/Module.js?v=18")
            }
        };
    }
}
