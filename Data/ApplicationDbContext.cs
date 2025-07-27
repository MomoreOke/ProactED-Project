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
        public DbSet<EquipmentUsageHistory> EquipmentUsageHistories { get; set; }

        public DbSet<EquipmentType> EquipmentTypes { get; set; }
        public DbSet<EquipmentModel> EquipmentModels { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<SavedDashboardView> SavedDashboardViews { get; set; }
        
        // Semester management
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<SemesterEquipmentUsage> SemesterEquipmentUsages { get; set; }

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

            // MaintenanceLog configuration - consolidated
            modelBuilder.Entity<MaintenanceLog>(entity =>
            {
                entity.HasKey(ml => ml.LogId);
                
                entity.HasOne(ml => ml.Equipment)
                    .WithMany(e => e.MaintenanceLogs)
                    .HasForeignKey(ml => ml.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ml => ml.Task)
                    .WithMany()
                    .HasForeignKey(ml => ml.MaintenanceTaskId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(ml => ml.Alert)
                    .WithMany()
                    .HasForeignKey(ml => ml.AlertId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

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

            // MaintenanceInventoryLink relationship configuration
            modelBuilder.Entity<MaintenanceInventoryLink>(entity =>
            {
                entity.HasKey(mil => mil.LinkId);
                
                entity.HasOne(mil => mil.MaintenanceLog)
                    .WithMany(ml => ml.MaintenanceInventoryLinks)
                    .HasForeignKey(mil => mil.LogId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(mil => mil.InventoryItem)
                    .WithMany(ii => ii.MaintenanceInventoryLinks)
                    .HasForeignKey(mil => mil.ItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Legacy relationships (keep for migration compatibility)
            modelBuilder.Entity<InventoryStock>()
                .HasOne(ist => ist.InventoryItem)
                .WithMany(ii => ii.InventoryStocks)
                .HasForeignKey(ist => ist.ItemId);

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

            // Semester configuration
            modelBuilder.Entity<Semester>(entity =>
            {
                entity.HasKey(s => s.SemesterId);
                
                entity.HasOne(s => s.UploadedBy)
                    .WithMany()
                    .HasForeignKey(s => s.UploadedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(s => s.SemesterName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.TotalEquipmentHours)
                    .HasPrecision(10, 2);
            });

            // SemesterEquipmentUsage configuration
            modelBuilder.Entity<SemesterEquipmentUsage>(entity =>
            {
                entity.HasKey(seu => seu.Id);
                
                entity.HasOne(seu => seu.Semester)
                    .WithMany(s => s.SemesterEquipmentUsages)
                    .HasForeignKey(seu => seu.SemesterId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(seu => seu.Equipment)
                    .WithMany()
                    .HasForeignKey(seu => seu.EquipmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(seu => seu.WeeklyUsageHours)
                    .HasPrecision(8, 2);

                entity.HasIndex(seu => new { seu.SemesterId, seu.EquipmentId })
                    .IsUnique()
                    .HasDatabaseName("IX_SemesterEquipmentUsage_Semester_Equipment");
            });

            // EquipmentUsageLog configuration - temporarily commented
            /*
            modelBuilder.Entity<EquipmentUsageLog>(entity =>
            {
                entity.HasKey(eul => eul.UsageId);
                
                entity.HasOne(eul => eul.Equipment)
                    .WithMany()
                    .HasForeignKey(eul => eul.EquipmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(eul => eul.HoursUsed)
                    .HasPrecision(5, 2);
                
                entity.Property(eul => eul.TemperatureReading)
                    .HasPrecision(5, 2);
                
                entity.Property(eul => eul.EfficiencyScore)
                    .HasPrecision(5, 2);
                
                entity.Property(eul => eul.PowerConsumption)
                    .HasPrecision(8, 2);
                
                entity.Property(eul => eul.VibrationLevel)
                    .HasPrecision(3, 2);
                
                entity.Property(eul => eul.MemoryUsagePercent)
                    .HasPrecision(5, 2);

                entity.HasIndex(eul => new { eul.EquipmentId, eul.UsageDate })
                    .HasDatabaseName("IX_EquipmentUsageLog_Equipment_Date");
            });
            */

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
                new EquipmentModel { EquipmentModelId = 1, EquipmentTypeId = 1, ModelName = "Black Dragon lenovo v4 projector" },
                new EquipmentModel { EquipmentModelId = 2, EquipmentTypeId = 2, ModelName = "21D model 6 Hisense air conditioner" },
                new EquipmentModel { EquipmentModelId = 3, EquipmentTypeId = 2, ModelName = "21D model 4 Hisense air conditioner" },
                new EquipmentModel { EquipmentModelId = 4, EquipmentTypeId = 1, ModelName = "2005 Metallic back grey projector" },
                new EquipmentModel { EquipmentModelId = 5, EquipmentTypeId = 3, ModelName = "XX5 Dragon Podium" }
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
