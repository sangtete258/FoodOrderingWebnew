using FoodOrderingWeb.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ShippingConfig> ShippingConfigs { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình relationships cho Food và Category
            modelBuilder.Entity<Food>()
                .HasOne(f => f.Category)
                .WithMany(c => c.Foods)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình relationships cho Order và OrderDetail
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Food)
                .WithMany()
                .HasForeignKey(od => od.FoodId);

            // Cấu hình relationships cho Order và StatusHistories
            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(osh => osh.Order)
                .WithMany(o => o.StatusHistories)
                .HasForeignKey(osh => osh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình decimal precision
            modelBuilder.Entity<Food>()
                .Property(f => f.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingFee)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.FinalTotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ShippingConfig>()
                .Property(s => s.ShippingFee)
                .HasColumnType("decimal(18,2)");

            // Cấu hình index cho tìm kiếm
            modelBuilder.Entity<Food>()
                .HasIndex(f => f.Name);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderCode);

            modelBuilder.Entity<ShippingConfig>()
                .HasIndex(s => s.AreaName);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasIndex(osh => osh.OrderId);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasIndex(osh => osh.ChangedDate);

            // Cấu hình các trường required
            modelBuilder.Entity<Order>()
                .Property(o => o.CustomerName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Order>()
                .Property(o => o.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Order>()
                .Property(o => o.DeliveryAddress)
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderCode)
                .IsRequired();

            modelBuilder.Entity<ShippingConfig>()
                .Property(s => s.AreaName)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}