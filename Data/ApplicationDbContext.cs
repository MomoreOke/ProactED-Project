using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Models;

namespace FEENALOoFINALE.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
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
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<MaintenanceTask> MaintenanceTasks { get; set; }

        public DbSet<EquipmentType> EquipmentTypes { get; set; }
        public DbSet<EquipmentModel> EquipmentModels { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Room> Rooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Equipment>()
                .HasKey(e => e.EquipmentId);

            modelBuilder.Entity<EquipmentType>()
                .HasKey(et => et.EquipmentTypeId);

            modelBuilder.Entity<EquipmentModel>()
                .HasKey(em => em.EquipmentModelId);

            modelBuilder.Entity<Building>()
                .HasKey(b => b.BuildingId);

            modelBuilder.Entity<Room>()
                .HasKey(r => r.RoomId);

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

            modelBuilder.Entity<MaintenanceTask>()
                .HasKey(m => m.TaskId);

            modelBuilder.Entity<MaintenanceTask>()
                .HasOne(m => m.AssignedTo)
                .WithMany(u => u.MaintenanceTasks)
                .HasForeignKey(m => m.AssignedToUserId);

            modelBuilder.Entity<InventoryStock>()
                .Property(s => s.Quantity)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<InventoryStock>()
                .Property(s => s.UnitCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasOne(d => d.EquipmentType)
                    .WithMany(p => p.Equipments)
                    .HasForeignKey(d => d.EquipmentTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.EquipmentModel)
                    .WithMany(p => p.Equipments)
                    .HasForeignKey(d => d.EquipmentModelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Building)
                    .WithMany(b => b.Equipments)
                    .HasForeignKey(d => d.BuildingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Room)
                    .WithMany(r => r.Equipments)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EquipmentModel>()
                .HasOne(em => em.EquipmentType)
                .WithMany(et => et.EquipmentModels)
                .HasForeignKey(em => em.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.Building)
                .WithMany(b => b.Rooms)
                .HasForeignKey(r => r.BuildingId);
        }
    }
}
