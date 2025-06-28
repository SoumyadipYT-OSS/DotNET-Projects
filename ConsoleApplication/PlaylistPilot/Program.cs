using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using PlaylistPilot.Data;
using PlaylistPilot.Services;


namespace PlaylistPilot;

class Program 
{
    static async Task Main(string[] args) {
        Console.WriteLine("Setting up PlaylistPilot...");

        // Build configuration to read appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // TODO: Will be replaced with full CLI implementation
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) => {
                // TODO: ADdd DbContext and services here

                // Register Entity Framework DbContext
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

                // Register CLI services
                services.AddScoped<ICliService, CliService>();
            })
            .Build();

        // Run the CLI application
        using var scope = host.Services.CreateScope();
        var cliService = scope.ServiceProvider.GetRequiredService<ICliService>();
        await cliService.RunAsync();
    }
}
