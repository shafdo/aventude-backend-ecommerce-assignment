using backend_assignment.Data;
using backend_assignment.Models;
using backend_assignment.Models.dtos.Category;
using backend_assignment.Models.dtos.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using Serilog;

namespace backend_assignment.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : Controller
    {
        private readonly EcommerceAPIDbContext dbContext;
        private readonly ILogger<AdminController> _logger;

        public ProductController(EcommerceAPIDbContext dbContext, ILogger<AdminController> logger)
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
        // Util Functions End


        [HttpGet]
        public async Task<IActionResult> GetAllProduct()
        {
            var products = await dbContext.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet]
        [Route("{ProductId:guid}")]
        public async Task<IActionResult> GetProduct([FromRoute] Guid ProductId)
        {
            var product = await dbContext.Products.FindAsync(ProductId);
            if (product == null) return NotFound("Product not found.");

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductCreateRequest productCreateRequest)
        {
            // Validations: Admin Check
            bool isAdmin = checkAdmin(GetRequest());
            if (!isAdmin) return Unauthorized("Please login as an admin.");

            // Validations
            if (productCreateRequest.ProductStock <= 0) return BadRequest("Stock must have a minimum of 1.");

            var productOrdersObject = new ProductJson()
            {
                ProductOrders = new List<Guid>()
            };

            var productOrdersJson = JsonConvert.SerializeObject(productOrdersObject);

            Guid productId = Guid.NewGuid();
            var product = new Product()
            {
                ProductId = productId,
                ProductName = productCreateRequest.ProductName,
                ProductDesc = productCreateRequest.ProductDesc,
                ProductStock = productCreateRequest.ProductStock,
                ProductCategoryId = null,
                ProductOrders = productOrdersJson
            };

            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();

            this._logger.LogInformation($"New product ({productId}) added.");

            return Ok("Product created successfully.");
        }


        [HttpPut]
        [Route("{ProductId:guid}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid ProductId, ProductUpdateRequest productUpdateRequest)
        {
            // Validations: Admin Check
            bool isAdmin = checkAdmin(GetRequest());
            if (!isAdmin) return Unauthorized("Please login as an admin.");

            // Validations: Validate product ID
            var product = await dbContext.Products.FindAsync(ProductId);
            if (product == null) return NotFound("Product not found.");

            // Validations: Validate Product category
            var productCategoryId = productUpdateRequest.ProductCategoryId;
            if (productCategoryId != null)
            {
                try
                {
                    Guid identifier = Guid.Parse(productCategoryId);
                    var userCount = dbContext.ProductCategorys.Count(x => x.ProductCategoryId == identifier);
                    if (userCount <= 0) return NotFound("Category not found. Please recheck your category ID or use null if you haven't decided a category yet.");

                }
                catch (Exception ex)
                {
                    return NotFound("Category not found. Please recheck your category ID or use null if you haven't decided a category yet.");
                }
            };

            product.ProductName = productUpdateRequest.ProductName;
            product.ProductDesc = productUpdateRequest.ProductDesc;
            product.ProductStock = productUpdateRequest.ProductStock;

            await dbContext.SaveChangesAsync();

            return Ok("Product updated sucessfully");
        }

        [HttpDelete]
        [Route("{ProductId:guid}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] Guid ProductId)
        {
            // Validations: Admin Check
            bool isAdmin = checkAdmin(GetRequest());
            if (!isAdmin) return Unauthorized("Please login as an admin.");

            var product = await dbContext.Products.FindAsync(ProductId);
            if (product == null) return NotFound("Product not found.");

            // Remove from category
            if (product.ProductCategoryId != null)
            {
                Guid categoryId = (Guid)product.ProductCategoryId;
                var category = dbContext.ProductCategorys.Find(categoryId);

                // Deserialize
                var categoryProductsJson = category.ProductCategoryProducts;
                CategoryProductJson categoryProductsObj = JsonConvert.DeserializeObject<CategoryProductJson>(categoryProductsJson, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                categoryProductsObj.products.Remove(ProductId);

                // Serialize to store in database
                categoryProductsJson = JsonConvert.SerializeObject(categoryProductsObj);
                category.ProductCategoryProducts = categoryProductsJson;
            }

            dbContext.Remove(product);
            await dbContext.SaveChangesAsync();

            this._logger.LogInformation($"Product ({product.ProductId}) removed from store.");

            return Ok("Product deleted successfully.");
        }
    }
}
