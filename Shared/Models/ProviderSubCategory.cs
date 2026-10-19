using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace OpenEugene.LittleHelpBook.Models
{
    [Table("ProviderSubCategory")]
    public class ProviderSubCategory : ModelBase
    {
        [Key]
        public int ProviderSubCategoryId { get; set; }
        public int ProviderId { get; set; }
        public int SubCategoryId { get; set; }
    }
}
