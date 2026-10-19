using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace OpenEugene.LittleHelpBook.Models
{
    [Table("PhoneNumber")]
    public class PhoneNumber : ModelBase
    {
        [Key]
        public int PhoneNumberId { get; set; }
        public int? ProviderId { get; set; }
        public int? CountryCode { get; set; }
        public int? AreaCode { get; set; }
        public string Number { get; set; }
        public int? Extension { get; set; }
        public string Description { get; set; }
        [Column("l10N")]
        public string L10N { get; set; }
        public bool? IsActive { get; set; } = true;
    }
}
