using Cramy.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;


namespace Cramy.Persistence.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<CramyDbContext>();
            await ctx.Database.MigrateAsync();

            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { "Admin", "Seller", "Customer" })
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole(role));

            var admin = await userMgr.FindByEmailAsync("admin@cramy.com");
            if (admin is null)
            {
                admin = new ApplicationUser { UserName = "admin@cramy.com", Email = "admin@cramy.com", EmailConfirmed = true };
                await userMgr.CreateAsync(admin, "Admin123!");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }

            if (!ctx.Categories.Any())
            {
                var cat = new Category { Name = "Aksesuarlar", Slug = "aksesuarlar" };
                ctx.Categories.Add(cat);
                ctx.Products.Add(new Product
                {
                    SKU = "ACC-001",
                    Name = "USB-C Kablo",
                    Description = "Dayanıklı şarj ve veri kablosu",
                    Price = 129.90m,
                    StockQuantity = 100,
                    Category = cat
                });
                await ctx.SaveChangesAsync();
            }
        }
    }

}
