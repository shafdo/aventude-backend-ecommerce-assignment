using backend_assignment.Data;
using backend_assignment.Models;
using backend_assignment.Models.dtos.Category;
using backend_assignment.Models.dtos.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Serilog;
using backend_assignment.Models.dtos.Response;

namespace backend_assignment.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        // Database Handler
        private readonly EcommerceAPIDbContext dbContext;
        private readonly ILogger<AdminController> _logger;

        public UserController(EcommerceAPIDbContext dbContext, ILogger<AdminController> logger)
        {
            this.dbContext = dbContext;
            _logger = logger;
        }

        private HttpRequest GetRequest()
        {
            return Request;
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
        public async Task<IActionResult> GetUser()
        {
            bool isUserLogin = checkUserAuth(GetRequest());
            bool isAdminLogin = checkAdmin(GetRequest());

            if (checkUserAuth(GetRequest()) == false && checkAdmin(GetRequest()) == false) return Unauthorized("Please login to access this endpoint.");

            Guid userId = getUserId(GetRequest());
            var user = dbContext.Users.Where(x => x.UserId == userId).FirstOrDefault();
            user.Password = "REDACTED";
            user.Token = "REDACTED";
            return Ok(user);
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterUser(UserCreateRequest userCreateRequest)
        {
            // Check user already exist
            var userCount = dbContext.Users.Count(x => x.Email == userCreateRequest.Email);
            if (userCount > 0) return Unauthorized("User already exists. Please use another email.");

            // Create orders
            var userOrdersObject = new UserOrdersJson()
            {
                orders = new List<Guid>()
            };
            var userOrdersJson = JsonConvert.SerializeObject(userOrdersObject);

            // Create User
            var hash = hashPassword(userCreateRequest.Password);
            string token = generateToken();

            Guid userId = Guid.NewGuid();
            var user = new User()
            {
                UserId = userId,
                Email = userCreateRequest.Email,
                IsAdmin = false,
                Password = hash,
                Token = token,
                UserOrders = userOrdersJson
            };

            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            this._logger.LogInformation($"New user registered with email {userCreateRequest.Email}.");

            return Ok("User created successfully.");
        }


        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginUser(UserLoginRequest userLoginRequest)
        {
            var user = dbContext.Users.Where(x => x.Email == userLoginRequest.Email).FirstOrDefault();
            
            // Check email exist
            if(user == null) return Unauthorized("Invalid email or password.");
            
            // Check if password matched
            if ( hashPassword(userLoginRequest.Password) != user.Password ) return Unauthorized("Invalid email or password.");

            // Generate Auth Token
            string token = generateToken();
            string authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userLoginRequest.Email}:{token}"));

            // Update Auth Token
            user.Token = token;

            // Save token in DB
            await dbContext.SaveChangesAsync();

            // Generate auth cookie to send in HTTP response
            var cookieOptions = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(1),
                Path = "/",
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            };
            Response.Cookies.Append("auth", authToken, cookieOptions);

            var res = new LoginResponse()
            {
                msg = "Logged in Successfully. Redirecting...",
                token = authToken,
            };


            return Ok(res);
        }

        [Route("logout")]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("auth");
            return Ok("User logged out successfully.");
        }
    }
}
