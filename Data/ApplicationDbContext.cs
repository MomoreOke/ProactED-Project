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

        // Main entities
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
        public DbSet<SavedDashboardView> SavedDashboardViews { get; set; }

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

            // Legacy relationships (keep for migration compatibility)
            modelBuilder.Entity<InventoryStock>()
                .HasOne(ist => ist.InventoryItem)
                .WithMany(ii => ii.InventoryStocks)
                .HasForeignKey(ist => ist.ItemId);

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

            // Enhanced workflow relationships
            modelBuilder.Entity<MaintenanceTask>()
                .HasOne(mt => mt.OriginatingAlert)
                .WithMany()
                .HasForeignKey(mt => mt.CreatedFromAlertId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MaintenanceLog>()
                .HasOne(ml => ml.Task)
                .WithMany()
                .HasForeignKey(ml => ml.TaskId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<InventoryStock>()
                .Property(s => s.Quantity)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<InventoryStock>()
                .Property(s => s.UnitCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Room>()
                .HasOne(r => r.Building)
                .WithMany(b => b.Rooms)
                .HasForeignKey(r => r.BuildingId)
                .IsRequired();

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
                .HasForeignKey(r => r.BuildingId)
                .IsRequired();

            // Seed Equipment Types
            modelBuilder.Entity<EquipmentType>().HasData(
                new EquipmentType { EquipmentTypeId = 1, EquipmentTypeName = "Projectors" },
                new EquipmentType { EquipmentTypeId = 2, EquipmentTypeName = "Air Conditioners" },
                new EquipmentType { EquipmentTypeId = 3, EquipmentTypeName = "Podiums" }
            );

            // Seed Buildings
            modelBuilder.Entity<Building>().HasData(
                new Building { BuildingId = 1, BuildingName = "Petroleum Building" },
                new Building { BuildingId = 2, BuildingName = "New Engineering Building" }
            );

            // Seed Rooms
            modelBuilder.Entity<Room>().HasData(
                new Room { RoomId = 1, BuildingId = 1, RoomName = "PB001" },
                new Room { RoomId = 2, BuildingId = 1, RoomName = "PB012" },
                new Room { RoomId = 3, BuildingId = 1, RoomName = "PB014" },
                new Room { RoomId = 4, BuildingId = 1, RoomName = "PB020" },
                new Room { RoomId = 5, BuildingId = 1, RoomName = "PB201" },
                new Room { RoomId = 6, BuildingId = 1, RoomName = "PB208" },
                new Room { RoomId = 7, BuildingId = 1, RoomName = "PB214" },
                new Room { RoomId = 8, BuildingId = 2, RoomName = "NEB-GF" },
                new Room { RoomId = 9, BuildingId = 2, RoomName = "NEB-FF1" },
                new Room { RoomId = 10, BuildingId = 2, RoomName = "NEB-FF2" },
                new Room { RoomId = 11, BuildingId = 2, RoomName = "NEB-SF" },
                new Room { RoomId = 12, BuildingId = 2, RoomName = "NEB-TF" }
            );

            // Seed Equipment Models
            modelBuilder.Entity<EquipmentModel>().HasData(
                new EquipmentModel { EquipmentModelId = 1, EquipmentTypeId = 1, ModelName = "Projector Model A" },
                new EquipmentModel { EquipmentModelId = 2, EquipmentTypeId = 1, ModelName = "Projector Model B" },
                new EquipmentModel { EquipmentModelId = 3, EquipmentTypeId = 2, ModelName = "Air Conditioner Model A" },
                new EquipmentModel { EquipmentModelId = 4, EquipmentTypeId = 2, ModelName = "Air Conditioner Model B" },
                new EquipmentModel { EquipmentModelId = 5, EquipmentTypeId = 3, ModelName = "Podium Model A" },
                new EquipmentModel { EquipmentModelId = 6, EquipmentTypeId = 3, ModelName = "Podium Model B" }
            );
        }

        public async Task<List<Equipment>> GetEquipmentWithDetailsAsync()
        {
            return await Equipment
                .Include(e => e.MaintenanceLogs)
                .Include(e => e.FailurePredictions)
                .Include(e => e.Alerts)
                .AsSplitQuery()
                .ToListAsync();
        }
    }
}
