using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using stock_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super_secret_key_123!"))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("WebClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Dummy login endpoint
app.MapPost("/login", async (LoginRequest request, AppDbContext db) =>
{
    try
    {
        var user = await db.Users
    .FirstOrDefaultAsync(u => u.username == request.Username && u.password == request.Password);

        if (user is not null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("super_secret_key_123!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);
            return Results.Ok(new { userId = user.userId, token = jwt });
        }
        return Results.Unauthorized();
    }
    catch (Exception)
    {
        return Results.BadRequest();
    }
});

app.MapPost("/api/order", async ([FromBody] OrderModel order, AppDbContext db) =>
{
    try
    {
        order.CreatedAt = DateTime.Now;
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return Results.Ok(order);
    }
    catch (Exception)
    {
        return Results.BadRequest(order.UserId);
    }
    
}).RequireAuthorization();

app.MapGet("/api/order/{userId}", async (int userId, AppDbContext db) =>
{
    var orders = await db.Orders.Where(o => o.UserId == userId).ToListAsync();
    return Results.Ok(orders);
}).RequireAuthorization();

app.MapGet("/api/order_unauthorized/{userId}", async (int userId, AppDbContext db) =>
{
    var orders = await db.Orders.Where(o => o.UserId == userId).ToListAsync();
    return Results.Ok(orders);
});

app.Run();

public record LoginRequest(string Username, string Password);


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<OrderModel> Orders => Set<OrderModel>();
    public DbSet<UserModel> Users => Set<UserModel>();
}

// Exposes the generated Program class so WebApplicationFactory<Program> can be used from tests.
public partial class Program { }