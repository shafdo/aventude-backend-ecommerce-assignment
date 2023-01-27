using backend_assignment.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Init logger
var projectRootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.IndexOf("bin\\")));
var _logger = new LoggerConfiguration().WriteTo.File($"{projectRootPath}\\Logs\\ApiLog-.log", rollingInterval: RollingInterval.Day).CreateLogger();
builder.Logging.AddSerilog(_logger);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Service CORS
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials();
}));

//builder.Services.AddDbContext<EcommerceAPIDbContext>(options => options.UseInMemoryDatabase("EcommerceDb"));
builder.Services.AddDbContext<EcommerceAPIDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("EcommerceApiConnectionString")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("corsapp");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
