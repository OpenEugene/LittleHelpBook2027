using System.Collections.Generic;
using Oqtane.Models;
using Oqtane.Themes;
using Oqtane.Shared;

namespace OpenEugene.Theme.LittleHelpBook
{
    // Site theme for the Little Help Book. It hosts the MudBlazor providers and
    // loads MudBlazor's assets, so modules only use MudBlazor components.
    public class ThemeInfo : ITheme
    {
        public Oqtane.Models.Theme Theme => new Oqtane.Models.Theme
        {
            Name = "OpenEugene LittleHelpBook",
            Version = "1.0.0",
            PackageName = "OpenEugene.Theme.LittleHelpBook",
            Resources = new List<Resource>()
            {
                // Bootstrap is still used by Oqtane's own controls (menu, control panel, ActionLink).
                new Stylesheet(Constants.BootstrapStylesheetUrl, Constants.BootstrapStylesheetIntegrity, "anonymous"),
                new Stylesheet("/_content/MudBlazor/MudBlazor.min.css"),
                new Stylesheet("~/Theme.css"),
                new Script(Constants.BootstrapScriptUrl, Constants.BootstrapScriptIntegrity, "anonymous"),
                // Site level so the server writes it into the page before MudBlazor's
                // interactive components start; page-level scripts load too late.
                new Script("/_content/MudBlazor/MudBlazor.min.js") { Level = ResourceLevel.Site }
            }
        };
    }
}
