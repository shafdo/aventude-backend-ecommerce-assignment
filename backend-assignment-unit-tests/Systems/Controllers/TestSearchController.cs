using backend_assignment.Controllers;
using backend_assignment.Data;
using backend_assignment.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace backend_assignment_unit_tests.Systems.Controllers
{
    public class TestSearchController
    {
        
        // Get the database context that needed to be passed to the controller constructor
        private async Task<DbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<EcommerceAPIDbContext>()
           .UseInMemoryDatabase(databaseName: "EcommerceDb")
           .Options;

            var databaseContext = new EcommerceAPIDbContext(options);
            databaseContext.Database.EnsureCreated();

            // Add product
            databaseContext.Products.Add(new Product()
            {
                ProductId = new Guid("c2bb10b2-fdbf-455e-b246-e1fc65dd758b"),
                ProductName = "Mango",
                ProductDesc = "Test description",
                ProductStock = 100,
                ProductCategoryId = null,
                ProductOrders = "{\"ProductOrders\":[]}"
            });
            await databaseContext.SaveChangesAsync();

            return databaseContext;
        }


        // Positive test case: Search for a product that exist. This will result a 200 (Ok) status code.
        [Fact]
        public async Task SearchProductName_ShouldReturn200Status()
        {

            //Arrange
            var dbContext = await GetDatabaseContext();
            using var searchController = new SearchController((EcommerceAPIDbContext)dbContext);

            // Act
            var result = (ObjectResult)await searchController.SearchProductName("Mango");

            // Assert
            result.StatusCode.Should().Be(200);

        }

        // Negative test case: Search for a product category that does not exist. This will result a 404 (NotFound) status code.
        [Fact]
        public async Task SearchProductCategory_ShouldReturn404Status()
        {

            //Arrange
            var dbContext = await GetDatabaseContext();
            using var searchController = new SearchController((EcommerceAPIDbContext)dbContext);

            // Act
            Guid categoryId = new Guid("00000000-0000-0000-0000-000000000000");     // This category ID does not exist
            var result = (ObjectResult)await searchController.SearchCategory(categoryId);

            // Assert
            result.StatusCode.Should().Be(404);

        }
    }
}
