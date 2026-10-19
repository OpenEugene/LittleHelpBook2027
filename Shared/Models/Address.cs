using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace OpenEugene.LittleHelpBook.Models
{
    [Table("Address")]
    public class Address : ModelBase
    {
        [Key]
        public int AddressId { get; set; }
        public int? ProviderId { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public bool HasWheelchairAccess { get; set; }
        public bool HasLanguageSupport { get; set; }
        public string Geocoding { get; set; }
        public float? Longitude { get; set; }
        public float? Latitude { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
