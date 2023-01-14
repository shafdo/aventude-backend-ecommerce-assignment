namespace backend_assignment.Models.dtos.Product
{
    public class ProductCreateRequest
    {
        public string ProductName { get; set; }
        public string ProductDesc { get; set; }
        public int ProductStock { get; set; }
    }
}
