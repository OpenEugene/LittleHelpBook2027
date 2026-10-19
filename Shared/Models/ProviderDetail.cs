using System.Collections.Generic;

namespace OpenEugene.LittleHelpBook.Models
{
    // A provider with its addresses, phone numbers and category picks, as
    // loaded and saved by the Provider edit page. Not an EF entity.
    public class ProviderDetail
    {
        public Provider Provider { get; set; } = new Provider();
        public List<Address> Addresses { get; set; } = new List<Address>();
        public List<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();
        public List<int> CategoryIds { get; set; } = new List<int>();
        public List<int> SubCategoryIds { get; set; } = new List<int>();
    }
}
