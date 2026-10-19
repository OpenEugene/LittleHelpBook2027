using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace OpenEugene.LittleHelpBook.Models
{
    [Table("Provider")]
    public class Provider : ModelBase
    {
        [Key]
        public int ProviderId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string WebAddress { get; set; }
        public string EmailAddress { get; set; }
        public string HoursOfOperation { get; set; }
        public bool IsActive { get; set; } = true;
        [Column("l10N")]
        public string L10N { get; set; }
    }
}
