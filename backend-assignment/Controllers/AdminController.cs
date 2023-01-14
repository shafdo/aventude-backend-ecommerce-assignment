using backend_assignment.Data;
using backend_assignment.Models;
using backend_assignment.Models.dtos.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace backend_assignment.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : Controller
    {

        private readonly EcommerceAPIDbContext dbContext;
        private readonly ILogger<AdminController> _logger;

        public AdminController(EcommerceAPIDbContext dbContext, ILogger<AdminController> _logger)
        {
            this.dbContext = dbContext;
            this._logger = _logger;
        }

        // Util Functions
        string hashPassword(string password)
        {
            var sha = SHA256.Create();
            var asByteArray = Encoding.Default.GetBytes(password);
            var hash = sha.ComputeHash(asByteArray);
            return Convert.ToBase64String(hash);
        }

        string generateToken()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join("", Enumerable.Repeat(0, 50).Select(n => (char)new Random().Next(127)))));
        }
        // Util Functions End


        [HttpGet]
        [Route("create")]
        public async Task<IActionResult> CreateAdminUser()
        {
            var userCount = dbContext.Users.Count(x => x.Email == "admin@aventude.com");

            if (userCount <= 0)
            {
                var test = new User()
                {
                    UserId = Guid.NewGuid(),
                    Email = "admin@aventude.com",
                    IsAdmin = true,
                    Password = hashPassword("Admin321"),
                    Token = generateToken(),
                    UserOrders = "{}"
                };

                await dbContext.Users.AddAsync(test);
                await dbContext.SaveChangesAsync();

                this._logger.LogInformation("Admin user created.");
                return Ok("Admin user created");
            }

            return Ok("Admin user already created");
        }
    }
}
