using auth_app_backend.Data; // Make sure this includes your ApplicationDbContext
using auth_app_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace auth_app_backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load and validate JWT Settings
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            if (string.IsNullOrEmpty(jwtSettings["Issuer"]) ||
                string.IsNullOrEmpty(jwtSettings["Audience"]) ||
                string.IsNullOrEmpty(jwtSettings["Key"]) ||
                string.IsNullOrEmpty(jwtSettings["ExpiresInMinutes"]))
            {
                throw new ArgumentNullException("JWT configuration is missing required fields in appsettings.json");
            }

            // Register HttpClient for Dependency Injection
            builder.Services.AddHttpClient<CouchDbService>(); // Register CouchDbService with HttpClient

            // Register CouchDbService for Dependency Injection (DI)
            builder.Services.AddScoped<CouchDbService>();

            // Configure JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
                };
            });

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:4200") // Ensure your Angular app is running here
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Configure PostgreSQL with ApplicationDbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<NoticeBoardContext>(options => options.UseNpgsql(connectionString));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Ensure the CouchDB database exists at startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var couchDbService = services.GetRequiredService<CouchDbService>();
                await couchDbService.EnsureDatabaseExistsAsync(); // Ensure the database exists
            }

            // Configure middleware
            app.UseSwagger();
            app.UseSwaggerUI();
            // app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}