using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using FocusFlow.Data;
using FocusFlow.Services;
using FocusFlow.CLI;

namespace FocusFlow {
    public class Program {
        static async Task Main(string[] args) {
            // Build the host and configure services
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) => {
                    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    // Register application services
                    services.AddSingleton<TimerService>();
                    services.AddSingleton<Menu>();
                    services.AddSingleton<SessionViewer>();
                })
                .Build();

            // Create a service scope
            using var scope = host.Services.CreateScope();

            // Ensure database is created (optional but useful for dev)
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            // Start the CLI menu
            var menu = scope.ServiceProvider.GetRequiredService<Menu>();
            await menu.ShowAsync();

            // Optional: test timer directly (for debugging)
            // var timer = scope.ServiceProvider.GetRequiredService<TimerService>();
            // await timer.StartTimerAsync(TimeSpan.FromSeconds(10), "Focus");

            Console.WriteLine("FocusFlow exited.");
        }
    }
}