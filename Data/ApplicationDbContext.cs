using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Models;

namespace FEENALOoFINALE.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
        public DbSet<FailurePrediction> FailurePredictions { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryStock> InventoryStocks { get; set; }
        public DbSet<MaintenanceInventoryLink> MaintenanceInventoryLinks { get; set; }
        public new DbSet<User> Users { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure primary keys
            modelBuilder.Entity<Equipment>()
                .HasKey(e => e.EquipmentId);

            modelBuilder.Entity<MaintenanceLog>()
                .HasKey(ml => ml.LogId);

            modelBuilder.Entity<FailurePrediction>()
                .HasKey(fp => fp.PredictionId);

            modelBuilder.Entity<InventoryItem>()
                .HasKey(ii => ii.ItemId);

            modelBuilder.Entity<InventoryStock>()
                .HasKey(ist => ist.StockId);

            modelBuilder.Entity<MaintenanceInventoryLink>()
                .HasKey(mil => mil.LinkId);

            modelBuilder.Entity<Alert>()
                .HasKey(a => a.AlertId);

            // Configure relationships
            modelBuilder.Entity<MaintenanceLog>()
                .HasOne(ml => ml.Equipment)
                .WithMany(e => e.MaintenanceLogs)
                .HasForeignKey(ml => ml.EquipmentId);

            modelBuilder.Entity<FailurePrediction>()
                .HasOne(fp => fp.Equipment)
                .WithMany(e => e.FailurePredictions)
                .HasForeignKey(fp => fp.EquipmentId);

            modelBuilder.Entity<Alert>()
                .HasOne(a => a.Equipment)
                .WithMany(e => e.Alerts)
                .HasForeignKey(a => a.EquipmentId);

            modelBuilder.Entity<InventoryStock>()
                .HasOne(ist => ist.InventoryItem)
                .WithMany(ii => ii.InventoryStocks)
                .HasForeignKey(ist => ist.ItemId);

            modelBuilder.Entity<MaintenanceInventoryLink>()
                .HasOne(mil => mil.MaintenanceLog)
                .WithMany(ml => ml.MaintenanceInventoryLinks)
                .HasForeignKey(mil => mil.LogId);

            modelBuilder.Entity<MaintenanceInventoryLink>()
                .HasOne(mil => mil.InventoryItem)
                .WithMany(ii => ii.MaintenanceInventoryLinks)
                .HasForeignKey(mil => mil.ItemId);

            modelBuilder.Entity<Alert>()
                .HasOne(a => a.AssignedTo)
                .WithMany(u => u.AssignedAlerts)
                .HasForeignKey(a => a.AssignedToUserId);
        }
    }
}
