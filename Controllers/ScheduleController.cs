using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Services;
using FEENALOoFINALE.ViewModels;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MaintenanceSchedulingService _schedulingService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(
            ApplicationDbContext context, 
            MaintenanceSchedulingService schedulingService,
            ILogger<ScheduleController> logger)
        {
            _context = context;
            _schedulingService = schedulingService;
            _logger = logger;
        }

        // GET: Schedule
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new ScheduleViewModel
                {
                    PageTitle = "Maintenance Schedule",
                    PageDescription = "View and manage maintenance schedules for all equipment"
                };

                // Get upcoming maintenance tasks (next 30 days)
                var upcomingTasks = await _context.MaintenanceTasks
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentModel)
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentType)
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.Room)
                            .ThenInclude(r => r!.Building)
                    .Include(mt => mt.AssignedTo)
                    .Where(mt => mt.Status == MaintenanceStatus.Pending && 
                                mt.ScheduledDate >= DateTime.Now && 
                                mt.ScheduledDate <= DateTime.Now.AddDays(30))
                    .OrderBy(mt => mt.ScheduledDate)
                    .ToListAsync();

                // Get overdue tasks
                var overdueTasks = await _context.MaintenanceTasks
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentModel)
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentType)
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.Room)
                            .ThenInclude(r => r!.Building)
                    .Include(mt => mt.AssignedTo)
                    .Where(mt => mt.Status == MaintenanceStatus.Pending && 
                                mt.ScheduledDate < DateTime.Now)
                    .OrderBy(mt => mt.ScheduledDate)
                    .ToListAsync();

                // Get in-progress tasks
                var inProgressTasks = await _context.MaintenanceTasks
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentModel)
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentType)
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.Room)
                            .ThenInclude(r => r!.Building)
                    .Include(mt => mt.AssignedTo)
                    .Where(mt => mt.Status == MaintenanceStatus.InProgress)
                    .OrderBy(mt => mt.ScheduledDate)
                    .ToListAsync();

                // Get recently completed tasks (last 7 days)
                var recentlyCompleted = await _context.MaintenanceTasks
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentModel)
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentType)
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.Room)
                            .ThenInclude(r => r!.Building)
                    .Include(mt => mt.AssignedTo)
                    .Where(mt => mt.Status == MaintenanceStatus.Completed && 
                                mt.CompletedDate >= DateTime.Now.AddDays(-7))
                    .OrderByDescending(mt => mt.CompletedDate)
                    .ToListAsync();

                viewModel.UpcomingTasks = upcomingTasks;
                viewModel.OverdueTasks = overdueTasks;
                viewModel.InProgressTasks = inProgressTasks;
                viewModel.RecentlyCompleted = recentlyCompleted;

                // Calculate statistics
                viewModel.Statistics = new ScheduleStatistics
                {
                    TotalUpcoming = upcomingTasks.Count,
                    TotalOverdue = overdueTasks.Count,
                    TotalInProgress = inProgressTasks.Count,
                    TotalCompletedThisWeek = recentlyCompleted.Count,
                    HighPriorityCount = upcomingTasks.Count(t => t.Priority == TaskPriority.High || t.Priority == TaskPriority.Critical),
                    MediumPriorityCount = upcomingTasks.Count(t => t.Priority == TaskPriority.Medium),
                    LowPriorityCount = upcomingTasks.Count(t => t.Priority == TaskPriority.Low)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading schedule dashboard");
                TempData["ErrorMessage"] = "Error loading schedule data. Please try again.";
                return View(new ScheduleViewModel 
                { 
                    PageTitle = "Maintenance Schedule",
                    PageDescription = "View and manage maintenance schedules for all equipment"
                });
            }
        }

        // GET: Schedule/Calendar
        public async Task<IActionResult> Calendar()
        {
            try
            {
                // Get all maintenance tasks for calendar view (next 3 months and past month)
                var calendarTasks = await _context.MaintenanceTasks
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentModel)
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentType)
                    .Include(mt => mt.AssignedTo)
                    .Where(mt => mt.ScheduledDate >= DateTime.Now.AddDays(-30) && 
                                mt.ScheduledDate <= DateTime.Now.AddDays(90))
                    .OrderBy(mt => mt.ScheduledDate)
                    .ToListAsync();

                var viewModel = new ScheduleCalendarViewModel
                {
                    PageTitle = "Maintenance Calendar",
                    PageDescription = "Calendar view of all scheduled maintenance tasks",
                    MaintenanceTasks = calendarTasks
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading calendar view");
                TempData["ErrorMessage"] = "Error loading calendar data. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Schedule/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var maintenanceTask = await _context.MaintenanceTasks
                .Include(mt => mt.Equipment)
                    .ThenInclude(e => e!.EquipmentModel)
                .Include(mt => mt.Equipment)
                    .ThenInclude(e => e!.EquipmentType)
                .Include(mt => mt.Equipment)
                    .ThenInclude(e => e!.Room)
                        .ThenInclude(r => r!.Building)
                .Include(mt => mt.AssignedTo)
                .Include(mt => mt.OriginatingAlert)
                .FirstOrDefaultAsync(mt => mt.TaskId == id);

            if (maintenanceTask == null)
            {
                return NotFound();
            }

            // Get related maintenance history for this equipment
            var maintenanceHistory = await _context.MaintenanceLogs
                .Include(ml => ml.Equipment)
                .Where(ml => ml.EquipmentId == maintenanceTask.EquipmentId)
                .OrderByDescending(ml => ml.LogDate)
                .Take(10)
                .ToListAsync();

            var viewModel = new MaintenanceTaskDetailsViewModel
            {
                MaintenanceTask = maintenanceTask,
                MaintenanceHistory = maintenanceHistory
            };

            return View(viewModel);
        }

        // POST: Schedule/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, MaintenanceStatus status)
        {
            try
            {
                var task = await _context.MaintenanceTasks
                    .Include(t => t.Equipment)
                        .ThenInclude(e => e.EquipmentModel)
                    .Include(t => t.Equipment)
                        .ThenInclude(e => e.Building)
                    .Include(t => t.Equipment)
                        .ThenInclude(e => e.Room)
                    .Include(t => t.AssignedTo)
                    .FirstOrDefaultAsync(t => t.TaskId == id);

                if (task == null)
                {
                    return NotFound();
                }

                var oldStatus = task.Status;
                task.Status = status;

                if (status == MaintenanceStatus.Completed)
                {
                    task.CompletedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // Email notifications are disabled
                _logger.LogInformation("Email notifications would be sent for task {TaskId} status change to {Status}", 
                    task.TaskId, status);

                _logger.LogInformation("Maintenance task {TaskId} status updated from {OldStatus} to {NewStatus}", 
                    id, oldStatus, status);

                TempData["SuccessMessage"] = $"Task status updated to {status}";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status for task {TaskId}", id);
                TempData["ErrorMessage"] = "Error updating task status. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: Schedule/CreateTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(CreateMaintenanceTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the form errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var task = await _schedulingService.CreateMaintenanceTaskAsync(
                    model.EquipmentId,
                    model.Description,
                    model.Priority
                );

                _context.MaintenanceTasks.Add(task);
                await _context.SaveChangesAsync();

                // Send email notification if technician is assigned
                if (!string.IsNullOrEmpty(task.AssignedToUserId))
                {
                    try
                    {
                        // Load full task details with navigation properties for email
                        var taskWithDetails = await _context.MaintenanceTasks
                            .Include(t => t.Equipment)
                                .ThenInclude(e => e.EquipmentModel)
                            .Include(t => t.Equipment)
                                .ThenInclude(e => e.EquipmentType)
                            .Include(t => t.Equipment)
                                .ThenInclude(e => e.Building)
                            .Include(t => t.Equipment)
                                .ThenInclude(e => e.Room)
                            .Include(t => t.AssignedTo)
                            .FirstOrDefaultAsync(t => t.TaskId == task.TaskId);

                        if (taskWithDetails?.Equipment != null)
                        {
                            // Email notifications are disabled
                            _logger.LogInformation("Enhanced assignment email would be sent for task {TaskId}", task.TaskId);
                        }
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send assignment email for task {TaskId}", task.TaskId);
                        // Don't fail the task creation if email fails
                    }
                }

                _logger.LogInformation("New maintenance task created for equipment {EquipmentId}", model.EquipmentId);
                TempData["SuccessMessage"] = "Maintenance task created successfully";
                return RedirectToAction(nameof(Details), new { id = task.TaskId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance task for equipment {EquipmentId}", model.EquipmentId);
                TempData["ErrorMessage"] = "Error creating maintenance task. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Schedule/GenerateRoutineSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRoutineSchedule()
        {
            try
            {
                var tasksCreated = await _schedulingService.ScheduleRoutineMaintenanceAsync();
                
                _logger.LogInformation("Routine maintenance scheduling completed, {TasksCreated} tasks created", tasksCreated);
                TempData["SuccessMessage"] = $"Generated {tasksCreated} new maintenance tasks from routine scheduling";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating routine maintenance schedule");
                TempData["ErrorMessage"] = "Error generating routine maintenance schedule. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Schedule API for calendar data
        [HttpGet]
        public async Task<IActionResult> GetCalendarEvents()
        {
            try
            {
                var tasks = await _context.MaintenanceTasks
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentModel)
                    .Include(mt => mt.Equipment)
                        .ThenInclude(e => e!.EquipmentType)
                    .Where(mt => mt.ScheduledDate >= DateTime.Now.AddDays(-30) && 
                                mt.ScheduledDate <= DateTime.Now.AddDays(90))
                    .ToListAsync();

                var events = tasks.Select(mt => new
                    {
                        id = mt.TaskId,
                        title = $"{mt.Equipment!.EquipmentModel!.ModelName} - {mt.Description}",
                        start = mt.ScheduledDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        backgroundColor = mt.Status switch
                        {
                            MaintenanceStatus.Pending => mt.ScheduledDate < DateTime.Now ? "#dc3545" : "#007bff", // Red for overdue, blue for pending
                            MaintenanceStatus.InProgress => "#ffc107", // Yellow for in progress
                            MaintenanceStatus.Completed => "#28a745", // Green for completed
                            MaintenanceStatus.Cancelled => "#6c757d", // Gray for cancelled
                            _ => "#007bff"
                        },
                        borderColor = "#ffffff",
                        textColor = "#ffffff",
                        extendedProps = new
                        {
                            status = mt.Status.ToString(),
                            priority = mt.Priority.ToString(),
                            equipmentName = mt.Equipment!.EquipmentModel!.ModelName,
                            equipmentType = mt.Equipment!.EquipmentType!.EquipmentTypeName,
                            description = mt.Description,
                            assignedTo = mt.AssignedTo != null ? mt.AssignedTo.UserName : "Unassigned"
                        }
                    })
                    .ToList();

                return Json(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching calendar events");
                return BadRequest("Error fetching calendar data");
            }
        }
    }
}
