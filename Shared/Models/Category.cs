using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace OpenEugene.LittleHelpBook.Models
{
    [Table("Category")]
    public class Category : ModelBase
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Column("l10N")]
        public string L10N { get; set; }
    }
}
