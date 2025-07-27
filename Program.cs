using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models; // Add this line
using FEENALOoFINALE.Services; // Add this line for background services
using FEENALOoFINALE.Hubs; // Add SignalR Hub
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Change from IdentityUser to your custom User class
// Around line 14, where builder.Services.AddDefaultIdentity<User> is called
builder.Services.AddDefaultIdentity<User>(options => 
{
    options.SignIn.RequireConfirmedAccount = false; // Disable for development/testing
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Register Email Service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Register Export Service
builder.Services.AddScoped<IExportService, ExportService>();

// Register Advanced Analytics Service
builder.Services.AddScoped<IAdvancedAnalyticsService, AdvancedAnalyticsService>();

// Register Performance Optimization Services (commented out until implementations are ready)
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();
// builder.Services.AddScoped<IOptimizedDashboardService, OptimizedDashboardService>();
builder.Services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();

// Register Maintenance Scheduling Service
builder.Services.AddScoped<MaintenanceSchedulingService>();

// Authentication is handled by ASP.NET Core Identity with cookie authentication

// Add SignalR services
builder.Services.AddSignalR();

// Register Predictive Maintenance Services (Phase 2)
// builder.Services.AddScoped<EquipmentUsageTrackingService>(); // Will be added after model integration
// builder.Services.AddScoped<BasicPredictionService>(); // Will be added after model integration

// Register Background Services
// TODO: These services need to be fixed - commenting out for now
builder.Services.AddHostedService<PredictiveAnalyticsService>();
builder.Services.AddHostedService<AutomatedAlertService>();
builder.Services.AddHostedService<ScheduledMaintenanceService>();
builder.Services.AddHostedService<EquipmentMonitoringService>();
builder.Services.AddHostedService<MaintenanceSchedulingBackgroundService>();

// Add Swagger/OpenAPI - TODO: Fix Swagger configuration
/*
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Predictive Maintenance API", 
        Version = "v1",
        Description = "API for Predictive Maintenance Management System",
        Contact = new OpenApiContact
        {
            Name = "Predictive Maintenance Team",
            Email = "support@predictivemaintenance.com"
        }
    });

    // Add JWT Authentication support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
*/

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    // TODO: Re-enable Swagger when dependencies are fixed
    /*
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Predictive Maintenance API V1");
        c.RoutePrefix = "api-docs";
    });
    */
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Comment out or remove this line
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}")
    .WithStaticAssets();

// Map SignalR Hub
app.MapHub<MaintenanceHub>("/maintenanceHub");

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
