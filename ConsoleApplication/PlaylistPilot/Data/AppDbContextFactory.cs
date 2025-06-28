using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PlaylistPilot.Data {
    // This will be discovered by the EF tools when you call Add-Migration
    public class AppDbContextFactory
        : IDesignTimeDbContextFactory<AppDbContext> {
        public AppDbContext CreateDbContext(string[] args) {
            // 1) Build config
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())    // make sure this points at your .csproj folder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            // 2) Pull the connection string
            var connStr = config.GetConnectionString("DefaultConnection");

            // 3) Configure the DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connStr);

            // 4) Return your context
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}