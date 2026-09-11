using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using stock_api.Models;

namespace stock_api.Tests
{
    /// <summary>
    /// Custom WebApplicationFactory that swaps the SQL Server AppDbContext for an
    /// EF Core InMemory database so tests never touch a real database.
    /// Each instance gets its own uniquely named database so tests don't leak state.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public string DatabaseName { get; } = Guid.NewGuid().ToString();

        // A known, seeded user available to tests that need to log in.
        public const string SeededUsername = "testuser";
        public const string SeededPassword = "password123";
        public int SeededUserId { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing AppDbContext registration (SQL Server) if present.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(DatabaseName);
                });

                // Build the service provider and seed known data for tests.
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();

                var seededUser = new UserModel
                {
                    username = SeededUsername,
                    password = SeededPassword
                };
                db.Users.Add(seededUser);
                db.SaveChanges();
                SeededUserId = seededUser.userId;
            });
        }
    }
}
