using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Services;

var builder = WebApplication.CreateBuilder(args);

// Define a specific CORS policy name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// 1. Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EduSyncDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddSingleton<EventHubSender>();

// 2. Add CORS services and define a policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(
                              "http://localhost:3000",
                              "https://localhost:3000",
                              "https://nice-grass-01890ff00.6.azurestaticapps.net"
                          ) // Add both HTTP and HTTPS and production static app
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials(); // Add this if you're using authentication/cookies
                      });
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<EduSync.Services.BlobStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline - MIDDLEWARE ORDER IS CRITICAL
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add static files if needed (uncomment if you serve static files)
// app.UseStaticFiles();

// Add routing explicitly (recommended for clarity)
app.UseRouting();

// 3. Use CORS - MUST come after UseRouting and before UseAuthentication/UseAuthorization
app.UseCors(MyAllowSpecificOrigins);

// No authentication middleware since JWT is not used

app.UseAuthorization();

app.MapControllers();

app.Run();
