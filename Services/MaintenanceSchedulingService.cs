using FEENALOoFINALE.Models;
using FEENALOoFINALE.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace FEENALOoFINALE.Services
{
    public class MaintenanceSchedulingService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public MaintenanceSchedulingService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Get recommended maintenance interval based on equipment type and usage
        /// </summary>
        public int GetRecommendedMaintenanceInterval(EquipmentType equipmentType, Equipment equipment)
        {
            // Default maintenance intervals based on equipment type
            var maintenanceIntervals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                // Classroom Equipment
                { "Projector", 90 },           // 3 months
                { "Computer", 180 },           // 6 months
                { "Laptop", 120 },             // 4 months
                { "Interactive Board", 120 },  // 4 months
                { "Audio System", 90 },        // 3 months
                { "Air Conditioner", 90 },     // 3 months
                { "Printer", 60 },             // 2 months
                { "Scanner", 90 },             // 3 months
                
                // Lab Equipment
                { "Microscope", 180 },         // 6 months
                { "Laboratory Equipment", 120 }, // 4 months
                { "Scientific Instrument", 90 }, // 3 months
                
                // Network Equipment
                { "Router", 365 },             // 1 year
                { "Switch", 365 },             // 1 year
                { "Access Point", 180 },       // 6 months
                
                // Furniture & Fixtures
                { "Desk", 730 },               // 2 years
                { "Chair", 365 },              // 1 year
                { "Whiteboard", 365 },         // 1 year
                
                // Default for unknown types
                { "Default", 180 }             // 6 months
            };

            // Try to find specific interval for equipment type
            var typeName = equipmentType?.EquipmentTypeName ?? "Default";
            if (maintenanceIntervals.TryGetValue(typeName, out int interval))
            {
                return interval;
            }

            // Adjust based on equipment age and status
            var daysSinceInstallation = equipment.InstallationDate.HasValue ? 
                (DateTime.Now - equipment.InstallationDate.Value).Days : 
                365; // Default to 1 year if no installation date
            var baseInterval = maintenanceIntervals["Default"];

            // Older equipment needs more frequent maintenance
            if (daysSinceInstallation > 1095) // 3+ years old
            {
                baseInterval = (int)(baseInterval * 0.75); // 25% more frequent
            }
            else if (daysSinceInstallation > 730) // 2+ years old
            {
                baseInterval = (int)(baseInterval * 0.85); // 15% more frequent
            }

            // Equipment with recent issues needs more frequent maintenance
            var recentIssues = _context.MaintenanceLogs
                .Where(ml => ml.EquipmentId == equipment.EquipmentId && 
                            ml.LogDate >= DateTime.Now.AddDays(-90))
                .Count();

            if (recentIssues > 2)
            {
                baseInterval = (int)(baseInterval * 0.7); // 30% more frequent
            }

            return Math.Max(baseInterval, 30); // Minimum 30 days
        }

        /// <summary>
        /// Calculate next maintenance date based on last maintenance and recommended interval
        /// </summary>
        public async Task<DateTime> CalculateNextMaintenanceDateAsync(int equipmentId)
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentType)
                .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);

            if (equipment == null)
                return DateTime.Now.AddDays(180); // Default 6 months

            // Get last maintenance date
            var lastMaintenance = await _context.MaintenanceLogs
                .Where(ml => ml.EquipmentId == equipmentId && ml.Status == MaintenanceStatus.Completed)
                .OrderByDescending(ml => ml.LogDate)
                .FirstOrDefaultAsync();

            var lastMaintenanceDate = lastMaintenance?.LogDate ?? 
                equipment.InstallationDate ?? 
                DateTime.Now.AddMonths(-6); // Default to 6 months ago if no dates available
            var interval = GetRecommendedMaintenanceInterval(equipment.EquipmentType!, equipment);

            return lastMaintenanceDate.AddDays(interval);
        }

        /// <summary>
        /// Find the best technician to assign to a maintenance task
        /// </summary>
        public async Task<string?> FindBestTechnicianAsync(Equipment equipment, TaskPriority priority)
        {
            // Get all users with Technician role
            var technicians = await _userManager.GetUsersInRoleAsync("Technician");
            
            if (!technicians.Any())
            {
                // Fallback to Maintenance role
                technicians = await _userManager.GetUsersInRoleAsync("Maintenance");
            }

            if (!technicians.Any())
            {
                return null; // No technicians available
            }

            // Get workload for each technician (active tasks)
            var technicianWorkloads = new List<(User Technician, int ActiveTasks, int OverdueTasks)>();

            foreach (var tech in technicians)
            {
                var activeTasks = await _context.MaintenanceTasks
                    .Where(mt => mt.AssignedToUserId == tech.Id && 
                                (mt.Status == MaintenanceStatus.Pending || mt.Status == MaintenanceStatus.InProgress))
                    .CountAsync();

                var overdueTasks = await _context.MaintenanceTasks
                    .Where(mt => mt.AssignedToUserId == tech.Id && 
                                mt.ScheduledDate < DateTime.Now && 
                                (mt.Status == MaintenanceStatus.Pending || mt.Status == MaintenanceStatus.InProgress))
                    .CountAsync();

                technicianWorkloads.Add((tech, activeTasks, overdueTasks));
            }

            // For critical tasks, prefer technician with least overdue tasks
            if (priority == TaskPriority.Critical)
            {
                return technicianWorkloads
                    .OrderBy(t => t.OverdueTasks)
                    .ThenBy(t => t.ActiveTasks)
                    .First().Technician.Id;
            }

            // For normal tasks, prefer technician with least total workload
            return technicianWorkloads
                .OrderBy(t => t.ActiveTasks)
                .ThenBy(t => t.OverdueTasks)
                .First().Technician.Id;
        }

        /// <summary>
        /// Create a properly scheduled and assigned maintenance task
        /// </summary>
        public async Task<MaintenanceTask> CreateMaintenanceTaskAsync(
            int equipmentId, 
            string description, 
            TaskPriority priority = TaskPriority.Medium,
            int? createdFromAlertId = null)
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentType)
                .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);

            if (equipment == null)
                throw new ArgumentException("Equipment not found", nameof(equipmentId));

            var scheduledDate = await CalculateNextMaintenanceDateAsync(equipmentId);
            var assignedUserId = await FindBestTechnicianAsync(equipment, priority);

            var maintenanceTask = new MaintenanceTask
            {
                EquipmentId = equipmentId,
                ScheduledDate = scheduledDate,
                Status = MaintenanceStatus.Pending,
                Description = description,
                AssignedToUserId = assignedUserId,
                CreatedFromAlertId = createdFromAlertId,
                Priority = priority
            };

            return maintenanceTask;
        }

        /// <summary>
        /// Schedule routine maintenance for all equipment
        /// </summary>
        public async Task<int> ScheduleRoutineMaintenanceAsync()
        {
            var equipmentNeedingMaintenance = await _context.Equipment
                .Include(e => e.EquipmentType)
                .Where(e => e.Status == EquipmentStatus.Active)
                .ToListAsync();

            var tasksCreated = 0;

            foreach (var equipment in equipmentNeedingMaintenance)
            {
                // Check if there's already a pending maintenance task
                var existingTask = await _context.MaintenanceTasks
                    .Where(mt => mt.EquipmentId == equipment.EquipmentId && 
                                (mt.Status == MaintenanceStatus.Pending || mt.Status == MaintenanceStatus.InProgress))
                    .AnyAsync();

                if (existingTask) continue;

                var nextMaintenanceDate = await CalculateNextMaintenanceDateAsync(equipment.EquipmentId);

                // Only create task if maintenance is due within next 30 days
                if (nextMaintenanceDate <= DateTime.Now.AddDays(30))
                {
                    var task = await CreateMaintenanceTaskAsync(
                        equipment.EquipmentId,
                        $"Routine maintenance for equipment ID {equipment.EquipmentId}",
                        TaskPriority.Medium
                    );

                    _context.MaintenanceTasks.Add(task);
                    tasksCreated++;
                }
            }

            if (tasksCreated > 0)
            {
                await _context.SaveChangesAsync();
            }

            return tasksCreated;
        }
    }
}
