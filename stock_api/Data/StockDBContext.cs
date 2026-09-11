using Microsoft.EntityFrameworkCore;
using stock_api.Models;

namespace stock_api.Data
{
    public partial class StockDBContext : DbContext
    {
        public StockDBContext(DbContextOptions<StockDBContext> options)
        : base(options)
        {
        }

        public DbSet<OrderModel> Orders => Set<OrderModel>();
        public DbSet<UserModel> Users => Set<UserModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderModel>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Symbol).HasMaxLength(10).IsRequired();
                entity.Property(o => o.Side).HasMaxLength(4).IsRequired();
                entity.Property(o => o.Price).HasColumnType("decimal(18,2)");
                entity.Property(o => o.CreatedAt).HasDefaultValueSql("GETDATE()");
            });
        }
    }
 }
