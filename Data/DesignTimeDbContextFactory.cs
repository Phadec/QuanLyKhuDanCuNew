using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace QuanLyKhuDanCu.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Create options builder
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            // Configure MySQL
            optionsBuilder.UseMySql(connectionString, 
                ServerVersion.AutoDetect(connectionString),
                options => options.EnableRetryOnFailure());

            // Return instance of ApplicationDbContext
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
