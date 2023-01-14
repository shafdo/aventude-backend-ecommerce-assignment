namespace backend_assignment.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }

        // JSON String
        public string UserOrders { get; set; }

    }
}
