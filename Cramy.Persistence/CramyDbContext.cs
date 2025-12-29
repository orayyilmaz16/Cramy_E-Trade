using Cramy.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Cramy.Persistence
{
    public class CramyDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public CramyDbContext(DbContextOptions<CramyDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<Product>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.SKU).HasMaxLength(64).IsRequired();
                e.HasIndex(x => x.SKU).IsUnique();
                e.HasOne(x => x.Category)
                 .WithMany(c => c.Products)
                 .HasForeignKey(x => x.CategoryId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            });

            b.Entity<Category>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(120).IsRequired();
                e.Property(x => x.Slug).HasMaxLength(140).IsRequired();
                e.HasIndex(x => x.Slug).IsUnique();
            });

            b.Entity<Card>(e =>
            {
                e.HasIndex(x => x.UserId).IsUnique();
                e.HasMany(c => c.Items)
                 .WithOne(i => i.Card)
                 .HasForeignKey(i => i.CardId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<CartItem>(e =>
            {
                e.Property(x => x.Quantity).IsRequired();
                e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            });

            b.Entity<OrderItem>(e =>
            {
                e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            });

            b.Entity(typeof(InventoryTransaction)).Property("Delta").IsRequired();
        }
    }

}
