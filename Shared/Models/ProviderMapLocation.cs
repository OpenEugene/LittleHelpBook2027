namespace OpenEugene.LittleHelpBook.Models
{
    // A geocoded provider address used by the provider list map. Not an EF entity.
    public class ProviderMapLocation
    {
        public int AddressId { get; set; }
        public string Address { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
    }
}
