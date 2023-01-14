namespace backend_assignment.Models.dtos.Order
{
    public class OrderCreateRequest
    {
        public Guid ProductId { get; set; }
        public int OrderQty { get; set; }
    }
}
