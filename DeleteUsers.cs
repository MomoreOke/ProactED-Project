using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using Microsoft.AspNetCore.Identity;
using FEENALOoFINALE.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

class DeleteUsersUtility
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Setup services
        var services = new ServiceCollection();
        
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Add Identity services
        services.AddIdentity<User, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            Console.WriteLine("Connecting to database...");

            // Get all users
            var users = await userManager.Users.ToListAsync();
            
            Console.WriteLine($"Found {users.Count} users in the database.");

            if (users.Count == 0)
            {
                Console.WriteLine("No users to delete.");
                return;
            }

            Console.WriteLine("Deleting all users...");

            foreach (var user in users)
            {
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    Console.WriteLine($"Deleted user: {user.UserName} ({user.Email})");
                }
                else
                {
                    Console.WriteLine($"Failed to delete user: {user.UserName} - {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            Console.WriteLine("All users have been deleted from the database.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
