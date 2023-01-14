using backend_assignment.Data;
using backend_assignment.Models.dtos.Category;
using backend_assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Globalization;
using System.Diagnostics.Metrics;
using Serilog;

namespace backend_assignment.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : Controller
    {
        private readonly EcommerceAPIDbContext dbContext;
        private readonly ILogger<AdminController> _logger;

        public CategoryController(EcommerceAPIDbContext dbContext, ILogger<AdminController> logger)
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
        public async Task<IActionResult> GetCategories()
        {
            // Public method
            var categories = await dbContext.ProductCategorys.ToListAsync();
            return Ok(categories);
        }

        [HttpGet]
        [Route("{CategoryId:guid}")]
        public async Task<IActionResult> GetCategory([FromRoute] Guid CategoryId)
        {
            // Public method
            var category = await dbContext.ProductCategorys.FindAsync(CategoryId);
            if (category == null) return NotFound("Category not found.");

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryCreateRequest categoryCreateRequest)
        {
            // Validations: Admin Check
            bool isAdmin = checkAdmin(GetRequest());
            if (!isAdmin) return Unauthorized("Please login as an admin.");

            // Create capitalized category name
            string ProductCategoryName = new CultureInfo("en-US").TextInfo.ToTitleCase(categoryCreateRequest.ProductCategoryName);

            // Validations: Check category exist
            var categoryCount = dbContext.ProductCategorys.Count(x => x.ProductCategoryName == ProductCategoryName);
            if (categoryCount > 0) return BadRequest("Category with this name already exists. Please use a different category name.");

            var productObject = new CategoryProductJson()
            {
                products = new List<Guid>()
            };

            var productJson = JsonConvert.SerializeObject(productObject);

            var category = new ProductCategory()
            {
                ProductCategoryId = Guid.NewGuid(),
                ProductCategoryName = ProductCategoryName,
                ProductCategoryDesc = categoryCreateRequest.ProductCategoryDesc,
                ProductCategoryProducts = productJson
            };

            await dbContext.ProductCategorys.AddAsync(category);
            await dbContext.SaveChangesAsync();

            this._logger.LogInformation($"New category '{categoryCreateRequest.ProductCategoryName}' created.");

            return Ok("Category created successfully.");
        }

        [HttpDelete]
        [Route("{CategoryId:guid}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid CategoryId)
        {
            // Validations: Admin Check
            bool isAdmin = checkAdmin(GetRequest());
            if (!isAdmin) return Unauthorized("Please login as an admin.");

            var category = await dbContext.ProductCategorys.FindAsync(CategoryId);
            if (category == null) return NotFound("Category not found.");

            dbContext.Remove(category);
            await dbContext.SaveChangesAsync();

            this._logger.LogInformation($"New category '{category.ProductCategoryName}' deleted.");

            return Ok("Category removed successfully.");
        }

        [Route("products/{CategoryId:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateCategoryProducts([FromRoute] Guid CategoryId, CategoryProductUpdateRequest categoryProductUpdateRequest)
        {
            // Validations: Admin Check
            bool isAdmin = checkAdmin(GetRequest());
            if (!isAdmin) return Unauthorized("Please login as an admin.");

            // Validations: Validate category ID
            var category = await dbContext.ProductCategorys.FindAsync(CategoryId);
            if (category == null) return NotFound("Category not found.");

            Guid productId = categoryProductUpdateRequest.ProductId;
            Guid productCategoryId = category.ProductCategoryId;

            // Check product id is valid
            var productCount = dbContext.Products.Count(x => x.ProductId == productId);
            if (productCount <= 0) return NotFound("Product not found. Please recheck your product ID.");

            // Deserialize JSON Object
            var categoryProductsJson = category.ProductCategoryProducts;
            CategoryProductJson categoryProductsObj = JsonConvert.DeserializeObject<CategoryProductJson>(categoryProductsJson, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            // Add new product to deserialized object
            categoryProductsObj.products.Add(productId);

            // Serialize Object
            categoryProductsJson = JsonConvert.SerializeObject(categoryProductsObj);

            // Update record in database
            category.ProductCategoryProducts = categoryProductsJson;

            // Update product table category
            var product = await dbContext.Products.FindAsync(productId);
            product.ProductCategoryId = productCategoryId;

            await dbContext.SaveChangesAsync();

            return Ok("Sucessfully added product to category.");
        }


        [Route("products/{CategoryId:guid}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCategoryProducts([FromRoute] Guid CategoryId, CategoryProductDeleteRequest categoryProductDeleteRequest)
        {
            // Validations: Admin Check
            bool isAdmin = checkAdmin(GetRequest());
            if (!isAdmin) return Unauthorized("Please login as an admin.");

            // Validations: Validate category ID
            var category = await dbContext.ProductCategorys.FindAsync(CategoryId);
            if (category == null) return NotFound("Category not found.");

            Guid productId = categoryProductDeleteRequest.ProductId;

            // Check product id is valid
            var productCount = dbContext.Products.Count(x => x.ProductId == productId);
            if (productCount <= 0) return NotFound("Product not found. Please recheck your product ID.");

            // Deserialize JSON Object
            var categoryProductsJson = category.ProductCategoryProducts;
            CategoryProductJson categoryProductsObj = JsonConvert.DeserializeObject<CategoryProductJson>(categoryProductsJson, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            // Add new product to deserialized object
            categoryProductsObj.products.Remove(productId);

            // Serialize Object
            categoryProductsJson = JsonConvert.SerializeObject(categoryProductsObj);

            // Update record in database
            category.ProductCategoryProducts = categoryProductsJson;

            // Update product table category
            var product = await dbContext.Products.FindAsync(productId);
            product.ProductCategoryId = null;

            await dbContext.SaveChangesAsync();

            return Ok("Sucessfully removed product from category.");
        }

    }
}
