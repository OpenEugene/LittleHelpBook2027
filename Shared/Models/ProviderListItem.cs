using System.Collections.Generic;

namespace OpenEugene.LittleHelpBook.Models
{
    // A row in the provider list: the provider and the categories it is in.
    // Not an EF entity.
    public class ProviderListItem
    {
        public int ProviderId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}
