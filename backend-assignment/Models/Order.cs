namespace backend_assignment.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public int OrderQty { get; set; }

        public Guid ProductId { get; set; }


        // Relationship: One (User)
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
