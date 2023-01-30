using backend_assignment.Data;
using backend_assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using backend_assignment.Models.dtos.Order;
using Newtonsoft.Json.Linq;
using backend_assignment.Models.dtos.Product;
using backend_assignment.Models.dtos.User;
using Serilog;

namespace backend_assignment.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : Controller
    {

        private readonly EcommerceAPIDbContext dbContext;
        private readonly ILogger<AdminController> _logger;

        public OrderController(EcommerceAPIDbContext dbContext, ILogger<AdminController> logger)
        {
            this.dbContext = dbContext;
            _logger = logger;
        }

        private HttpRequest GetRequest()
        {
            return Request;
        }

        // Util Functions
        bool checkAdmin(HttpRequest request)
        {
            var cookie = request.Cookies["auth"];
            if (cookie == null) return false;

            // Extract authtoken
            try
            {
                var cookieDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(cookie));
                string email = cookieDecoded.Split(":")[0];
                string token = cookieDecoded.Split(":")[1];

                // Verify token and ensure the user is an admin
                var user = dbContext.Users.Where(x => x.Email == email).FirstOrDefault();
                if (user == null || user.Token != token || user.IsAdmin == false) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        bool checkUserAuth(HttpRequest request)
        {
            var cookie = request.Cookies["auth"];
            if (cookie == null) return false;

            // Extract authtoken
            try
            {
                var cookieDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(cookie));
                string email = cookieDecoded.Split(":")[0];
                string token = cookieDecoded.Split(":")[1];

                // Verify token and ensure the user is an admin
                var user = dbContext.Users.Where(x => x.Email == email).FirstOrDefault();
                if (user == null || user.Token != token || user.IsAdmin == true) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        Guid getUserId(HttpRequest request)
        {
            var cookie = request.Cookies["auth"];
            var cookieDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(cookie));
            string email = cookieDecoded.Split(":")[0];
            string token = cookieDecoded.Split(":")[1];
            var user = dbContext.Users.Where(x => x.Email == email).FirstOrDefault();
            return user.UserId;
        }
        // Util Functions End


        [HttpGet]
        [Route("{OrderId:guid}")]
        public async Task<IActionResult> GetOrder([FromRoute] Guid OrderId)
        {
            // Validations: User Check
            bool isUserAuthenticated = checkUserAuth(GetRequest());
            if (!isUserAuthenticated) return Unauthorized("Please login as a customer to create an order.");

            var order = dbContext.Orders.Where(x => x.OrderId == OrderId).FirstOrDefault();
            if (order == null) return NotFound("Order not found.");

            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderCreateRequest orderCreateRequest)
        {
            Console.WriteLine("Executing create order");
            // Validations: User Check
            bool isUserAuthenticated = checkUserAuth(GetRequest());
            if (!isUserAuthenticated) return Unauthorized("Please login as a customer to create an order.");

            // OrderQty check
            if (orderCreateRequest.OrderQty <= 0) return BadRequest("Order quantity must be at least 1");

            // Product
            Guid productId = orderCreateRequest.ProductId;

            // Product: Check product id is valid
            var product = dbContext.Products.Where(x => x.ProductId == productId).FirstOrDefault();
            if (product == null) return NotFound("Product not found."); ;

            // Product: Check product stock
            if (product.ProductStock <= 0) return BadRequest("Product out of stock.");

            // Product: Reduce product stock from order quantity
            int orginalStock = product.ProductStock;
            product.ProductStock = product.ProductStock - orderCreateRequest.OrderQty;

            if (product.ProductStock <= -1) return BadRequest($"Sorry. We only have {orginalStock} units remaining in stock for this product.");

            // Order
            Guid userId = getUserId(GetRequest());
            Guid orderId = Guid.NewGuid();
            var order = new Order()
            {
                OrderId = orderId,
                OrderDate = DateTime.Now,
                OrderStatus = "Processing",
                OrderQty = orderCreateRequest.OrderQty,
                UserId = userId,
                ProductId = productId,
                User = dbContext.Users.Where(x => x.UserId == userId).FirstOrDefault()
            };

            // Product: Add to order list in product model
            var productOrdersJson = product.ProductOrders;
            ProductJson productOrdersObj = JsonConvert.DeserializeObject<ProductJson>(productOrdersJson, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            productOrdersObj.ProductOrders.Add(orderId);
            productOrdersJson = JsonConvert.SerializeObject(productOrdersObj);
            product.ProductOrders = productOrdersJson;

            // User: Add to order list in user model
            var user = await dbContext.Users.FindAsync(userId);
            var userOrdersJson = user.UserOrders;
            UserOrdersJson userOrdersObj = JsonConvert.DeserializeObject<UserOrdersJson>(userOrdersJson, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            userOrdersJson = JsonConvert.SerializeObject(userOrdersObj);
            user.UserOrders = productOrdersJson;

            // Reflect the changes in the database
            await dbContext.Orders.AddAsync(order);
            await dbContext.SaveChangesAsync();


            this._logger.LogInformation($"New order created by user {user.Email}.");

            return Ok("Order created successfully.");
        }


        [HttpPut]
        [Route("{OrderId:guid}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid OrderId, OrderUpdateRequest orderUpdateRequest)
        {
            // Validations: Admin Check
            bool isAdmin = checkAdmin(GetRequest());
            if (!isAdmin) return Unauthorized("Please login as an admin.");

            // Validations: Validate product ID
            var order = await dbContext.Orders.FindAsync(OrderId);
            if (order == null) return NotFound("Order not found.");

            string status = orderUpdateRequest.status.ToLower();
            bool statusCheck = false;

            switch (status)
            {
                case "processing":
                    statusCheck = true; break;

                case "shipped":
                    statusCheck = true; break;

                case "complete":
                    statusCheck = true; break;
            }

            if (!statusCheck) return BadRequest("Invalid status. Please choose from 'processing | shipped | complete'.");

            order.OrderStatus = status;

            await dbContext.SaveChangesAsync();

            this._logger.LogInformation($"Order ({OrderId}) status change.");

            return Ok("Order status updated sucessfully.");
        }


    }
}
