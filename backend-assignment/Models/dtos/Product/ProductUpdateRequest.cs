namespace backend_assignment.Models.dtos.Product
{
    public class ProductUpdateRequest
    {
        public string ProductName { get; set; }
        public string ProductDesc { get; set; }
        public int ProductStock { get; set; }
        public string? ProductCategoryId { get; set; }
    }
}
