using backend_assignment.Models;

namespace backend_assignment.Models.dtos.Order
{
    public class UserOrdersJson
    {
        public Models.Order Order { get; set; }
        public Models.Product Product { get; set; }
    }
}
