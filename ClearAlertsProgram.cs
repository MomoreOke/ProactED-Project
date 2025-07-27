using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;

namespace FEENALOoFINALE
{
    public class ClearAlertsProgram
    {
        public static async Task Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ProjectDb;Trusted_Connection=true;MultipleActiveResultSets=true");

            using var context = new ApplicationDbContext(optionsBuilder.Options);

            Console.WriteLine("Clearing existing alerts...");

            // Clear all existing alerts
            var existingAlerts = await context.Alerts.ToListAsync();
            context.Alerts.RemoveRange(existingAlerts);
            await context.SaveChangesAsync();

            Console.WriteLine($"Cleared {existingAlerts.Count} alerts.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
