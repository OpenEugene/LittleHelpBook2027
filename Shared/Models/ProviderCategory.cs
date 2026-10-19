using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace OpenEugene.LittleHelpBook.Models
{
    [Table("ProviderCategory")]
    public class ProviderCategory : ModelBase
    {
        [Key]
        public int ProviderCategoryId { get; set; }
        public int ProviderId { get; set; }
        public int CategoryId { get; set; }
    }
}
