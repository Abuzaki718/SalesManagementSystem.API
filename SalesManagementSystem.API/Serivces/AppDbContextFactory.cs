using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SalesManagementSystem.EF.DataContext;

namespace SalesManagementSystem.API.Serivces
{

    /// <summary>
    /// This class is Added becozue of error happend when add migration 
    /// </summary>
    public class AppDbContextFactory() : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            // Replace with your actual connection string or retrieve it from configuration.
            optionsBuilder.UseSqlServer("Server= .; Database=SalesManagementSystemDb; TrustServerCertificate=True; Integrated Security=True;",
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

            return new AppDbContext(optionsBuilder.Options);
        }
    }

}
