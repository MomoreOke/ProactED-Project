# PowerShell script to add test data using Entity Framework

# This script will create sample equipment and alerts for testing
$currentDir = Get-Location
Set-Location "C:\Users\NABILA\Desktop\predictive-maintenance-system"

# Run Entity Framework commands to add test data
Write-Host "Adding test equipment and alerts..."

# Create a simple C# program to add test data
$testDataProgram = @"
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Add test data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Ensure database is created
    context.Database.EnsureCreated();
    
    // Add sample equipment if none exists
    if (!context.Equipment.Any())
    {
        var equipment1 = new Equipment
        {
            EquipmentTypeId = 1, // Projectors
            EquipmentModelId = 1, // Projector Model A
            BuildingId = 1, // Petroleum Building
            RoomId = 1, // PB001
            InstallationDate = DateTime.Now.AddYears(-2),
            ExpectedLifespanMonths = 60,
            Status = EquipmentStatus.Active,
            Notes = "Classroom projector"
        };

        var equipment2 = new Equipment
        {
            EquipmentTypeId = 2, // Air Conditioners
            EquipmentModelId = 3, // Air Conditioner Model A
            BuildingId = 1, // Petroleum Building
            RoomId = 2, // PB012
            InstallationDate = DateTime.Now.AddYears(-1),
            ExpectedLifespanMonths = 120,
            Status = EquipmentStatus.Active,
            Notes = "Main classroom AC unit"
        };

        context.Equipment.AddRange(equipment1, equipment2);
        context.SaveChanges();
        
        Console.WriteLine("Added sample equipment");
    }
    
    // Add sample alerts if none exists
    if (!context.Alerts.Any())
    {
        var equipment = context.Equipment.First();
        
        var alert1 = new Alert
        {
            EquipmentId = equipment.EquipmentId,
            Title = "Equipment Maintenance Required",
            Description = "Projector showing signs of overheating - bulb needs replacement",
            Priority = AlertPriority.High,
            Status = AlertStatus.Open,
            CreatedDate = DateTime.Now.AddDays(-2)
        };

        var alert2 = new Alert
        {
            EquipmentId = equipment.EquipmentId + 1,
            Title = "Filter Replacement Due",
            Description = "Air conditioner filter needs to be replaced as per schedule",
            Priority = AlertPriority.Medium,
            Status = AlertStatus.Open,
            CreatedDate = DateTime.Now.AddDays(-1)
        };

        context.Alerts.AddRange(alert1, alert2);
        context.SaveChanges();
        
        Console.WriteLine("Added sample alerts");
    }
    
    Console.WriteLine("Test data setup complete!");
}

app.Run();
"@

# Create a temporary test data program
$testDataProgram | Out-File -FilePath "TestDataProgram.cs" -Encoding UTF8

# Restore to original directory
Set-Location $currentDir

Write-Host "Test data script created. To add test data, run the TestDataProgram.cs"
