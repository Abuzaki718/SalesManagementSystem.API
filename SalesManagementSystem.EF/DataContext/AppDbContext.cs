using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Murur.Core.Domain.Identity;
using SalesManagementSystem.Core.Entities;

namespace SalesManagementSystem.EF.DataContext;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        #region Product 

        builder.Entity<Product>()
            .HasKey(p => p.ProductId);

        builder.Entity<Product>().Property(x => x.Price).HasColumnType("decimal(18,2)");


        builder.Entity<Product>()
            .HasMany(x => x.OrderItems).WithOne(x => x.Product).HasForeignKey(x => x.ProductId).OnDelete(deleteBehavior: DeleteBehavior.Restrict);
        #endregion

        #region Order

        builder.Entity<Order>()
            .HasKey(o => o.OrderId);

        builder.Entity<Order>().Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");


        builder.Entity<Order>()
           .HasMany(x => x.OrderItems).WithOne(x => x.Order).HasForeignKey(x => x.OrderId).OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        #endregion

        #region OrderItem

        builder.Entity<OrderItem>()
            .HasKey(o => o.OrderItemId);

        builder.Entity<OrderItem>().Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        #endregion


        base.OnModelCreating(builder);
    }

}
