using System.Collections.Generic;

namespace OpenEugene.LittleHelpBook.Models
{
    // A category with its subcategories. Not an EF entity.
    public class CategoryTree
    {
        public Category Category { get; set; }
        public List<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }
}
