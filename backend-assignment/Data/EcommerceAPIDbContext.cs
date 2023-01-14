using backend_assignment.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_assignment.Data
{
    public class EcommerceAPIDbContext : DbContext
    {
        // Create constructor that passin options and the options are passin to the base class.
        public EcommerceAPIDbContext(DbContextOptions options) : base(options)
        {
        }

        // Create DB Table handlers
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategorys { get; set; }
        public DbSet<Order> Orders { get; set; }

    }
}
