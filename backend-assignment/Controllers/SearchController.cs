using backend_assignment.Data;
using backend_assignment.Models;
using backend_assignment.Models.dtos.Category;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace backend_assignment.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : Controller
    {

        private readonly EcommerceAPIDbContext dbContext;
        public SearchController(EcommerceAPIDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private HttpRequest GetRequest()
        {
            return Request;
        }

        [HttpGet]
        [Route("{ProductName}")]
        public async Task<IActionResult> SearchProductName([FromRoute] string ProductName)
        {
            var products = await dbContext.Products.Where(x => x.ProductName.StartsWith(ProductName)).ToListAsync();
            if (products.Count <= 0) return NotFound("No products found.");
            return Ok(products);
        }


        [HttpGet]
        [Route("category/{CategoryId:guid}")]
        public async Task<IActionResult> SearchCategory([FromRoute] Guid CategoryId)
        {
            var category = await dbContext.ProductCategorys.FindAsync(CategoryId);
            if (category == null) return NotFound("Category not found.");

            // Deserialize JSON Object
            var categoryProductsJson = category.ProductCategoryProducts;
            CategoryProductJson categoryProductsObj = JsonConvert.DeserializeObject<CategoryProductJson>(categoryProductsJson, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            // Category product array empty return an empty list
            if (categoryProductsObj.products.Count <= 0) return Ok(new List<string>());


            List<Product> productsArr = new List<Product>();

            foreach (var product in categoryProductsObj.products)
            {
                Guid productId = product;
                var info = await dbContext.Products.FindAsync(productId);
                productsArr.Add(info);
            }

            return Ok(productsArr);
        }
    }
}
