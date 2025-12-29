using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace Cramy.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CramyDbContext>
    {
        public CramyDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CramyDbContext>();
            // Migration sırasında kullanılacak connection string
            optionsBuilder.UseSqlServer("Server=ORAY\\SQLEXPRESS;Database=CramyDb;Trusted_Connection=True;Encrypt=False");

            return new CramyDbContext(optionsBuilder.Options);
        }
    }

}
