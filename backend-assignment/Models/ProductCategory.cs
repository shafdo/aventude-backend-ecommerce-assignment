namespace backend_assignment.Models
{
    public class ProductCategory
    {
        public Guid ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public string ProductCategoryDesc { get; set; }

        // JSON String
        public string ProductCategoryProducts { get; set; }
    }
}
