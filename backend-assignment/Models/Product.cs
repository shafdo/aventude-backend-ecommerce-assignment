using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_assignment.Models
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDesc { get; set; }
        public int ProductStock { get; set; }
        public int ProductPrice { get; set; }
        public Guid? ProductCategoryId { get; set; }

        // JSON String
        public string ProductOrders { get; set; }

    }
}
