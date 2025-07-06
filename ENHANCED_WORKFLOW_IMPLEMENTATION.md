# 🔄 Enhanced Alert-to-Maintenance Workflow Implementation

## **Your Vision: Complete Automated Workflow**

```
Equipment Added → Monitoring → Issue Detected → Alert Created → 
Auto-Generate Maintenance Task (Pending) → Work Completed → 
Maintenance Log Created → Alert Status: Resolved
```

## **Implementation Steps**

### **Step 1: Enhance AutomatedAlertService**

Add automatic maintenance task creation when alerts are generated:

```csharp
// In AutomatedAlertService.cs
private async Task CheckAndGenerateAlerts()
{
    // ...existing alert generation code...
    
    // NEW: Auto-create maintenance tasks for critical alerts
    await AutoCreateMaintenanceTasksFromAlerts(dbContext, newAlerts);
}

private async Task AutoCreateMaintenanceTasksFromAlerts(ApplicationDbContext dbContext, List<Alert> newAlerts)
{
    var maintenanceTasks = new List<MaintenanceTask>();
    
    foreach (var alert in newAlerts.Where(a => a.Priority == AlertPriority.High || a.Priority == AlertPriority.Medium))
    {
        if (alert.EquipmentId.HasValue)
        {
            // Create corresponding maintenance task
            var task = new MaintenanceTask
            {
                EquipmentId = alert.EquipmentId.Value,
                Description = $"Address Alert: {alert.Description}",
                ScheduledDate = DateTime.Now.AddDays(GetMaintenanceUrgency(alert.Priority)),
                Status = MaintenanceStatus.Pending,
                Priority = MapAlertPriorityToTaskPriority(alert.Priority),
                CreatedFromAlertId = alert.AlertId  // Link back to original alert
            };
            
            maintenanceTasks.Add(task);
        }
    }
    
    if (maintenanceTasks.Any())
    {
        dbContext.MaintenanceTasks.AddRange(maintenanceTasks);
        await dbContext.SaveChangesAsync();
        
        // Notify dashboard of new pending tasks
        await _hubContext.Clients.All.SendAsync("NewMaintenanceTasksFromAlerts", maintenanceTasks.Count);
    }
}

private int GetMaintenanceUrgency(AlertPriority priority)
{
    return priority switch
    {
        AlertPriority.High => 1,     // Schedule for tomorrow
        AlertPriority.Medium => 3,   // Schedule for 3 days
        AlertPriority.Low => 7,      // Schedule for 1 week
        _ => 7
    };
}
```

### **Step 2: Update MaintenanceTask Model**

Add connection back to originating alert:

```csharp
// In MaintenanceTask.cs
public class MaintenanceTask
{
    // ...existing properties...
    
    public int? CreatedFromAlertId { get; set; }  // NEW: Link to originating alert
    public Alert? OriginatingAlert { get; set; }  // NEW: Navigation property
    public TaskPriority Priority { get; set; }    // NEW: Task priority
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}
```

### **Step 3: Enhanced Dashboard Display**

Show the complete workflow status:

```html
<!-- In Dashboard/Index.cshtml -->
<div class="workflow-status">
    <h5>Equipment Issue Workflow</h5>
    
    <!-- Step 1: Alerts -->
    <div class="step">
        <span class="step-number">1</span>
        <span class="step-title">Active Alerts</span>
        <span class="badge bg-danger">@Model.ActiveAlerts</span>
    </div>
    
    <!-- Step 2: Pending Tasks -->
    <div class="step">
        <span class="step-number">2</span>
        <span class="step-title">Pending Maintenance</span>
        <span class="badge bg-warning">@Model.PendingTasks</span>
    </div>
    
    <!-- Step 3: In Progress -->
    <div class="step">
        <span class="step-number">3</span>
        <span class="step-title">In Progress</span>
        <span class="badge bg-info">@Model.InProgressTasks</span>
    </div>
    
    <!-- Step 4: Completed -->
    <div class="step">
        <span class="step-number">4</span>
        <span class="step-title">Resolved Today</span>
        <span class="badge bg-success">@Model.ResolvedToday</span>
    </div>
</div>
```

### **Step 4: Enhanced Maintenance Log Creation**

When creating maintenance log, automatically complete the linked task:

```csharp
// In MaintenanceLogController.cs
public async Task<IActionResult> Create([Bind("EquipmentId,LogDate,MaintenanceType,Description,Technician,DowntimeHours,AlertId,TaskId")] MaintenanceLog maintenanceLog)
{
    if (ModelState.IsValid)
    {
        // ...existing save logic...
        
        // NEW: Complete linked maintenance task
        if (maintenanceLog.TaskId.HasValue)
        {
            var task = await _context.MaintenanceTasks.FindAsync(maintenanceLog.TaskId.Value);
            if (task != null)
            {
                task.Status = MaintenanceStatus.Completed;
                task.CompletedDate = DateTime.Now;
                _context.Update(task);
            }
        }
        
        // Resolve linked alert
        if (maintenanceLog.AlertId.HasValue)
        {
            var alert = await _context.Alerts.FindAsync(maintenanceLog.AlertId.Value);
            if (alert != null)
            {
                alert.Status = AlertStatus.Resolved;
                _context.Update(alert);
            }
        }
        
        await _context.SaveChangesAsync();
    }
}
```

### **Step 5: Real-time Workflow Updates**

Update dashboard in real-time as workflow progresses:

```javascript
// In Dashboard JavaScript
connection.on("NewAlert", function (alert) {
    updateWorkflowStep(1, +1); // Increment alerts
    showNotification("New Alert", alert.description, "warning");
});

connection.on("NewMaintenanceTasksFromAlerts", function (count) {
    updateWorkflowStep(2, count); // Add to pending tasks
    showNotification("Tasks Scheduled", `${count} maintenance tasks auto-scheduled`, "info");
});

connection.on("TaskCompleted", function (taskId) {
    updateWorkflowStep(2, -1); // Remove from pending
    updateWorkflowStep(4, +1);  // Add to completed
    showNotification("Task Completed", "Maintenance task completed successfully", "success");
});
```

## **Configuration Options**

### **Auto-Task Creation Settings**

```csharp
// In appsettings.json
{
  "MaintenanceWorkflow": {
    "AutoCreateTasks": true,
    "HighPriorityTaskDays": 1,
    "MediumPriorityTaskDays": 3,
    "LowPriorityTaskDays": 7,
    "RequireApprovalForCritical": true
  }
}
```

### **Alert-to-Task Mapping Rules**

```csharp
public static class AlertToTaskMapping
{
    public static string GenerateTaskDescription(Alert alert)
    {
        return alert.Description switch
        {
            var desc when desc.Contains("overdue maintenance") => "Perform overdue maintenance inspection",
            var desc when desc.Contains("inactive") => "Investigate equipment status and restore operation",
            var desc when desc.Contains("failure prediction") => "Conduct preventive maintenance to avoid failure",
            var desc when desc.Contains("inventory") => "Check parts availability and restock if needed",
            _ => $"Address equipment issue: {alert.Description}"
        };
    }
}
```

## **Benefits of This Enhanced Workflow**

1. **Fully Automated**: Equipment issues → Alerts → Tasks → Completion
2. **Traceable**: Complete audit trail from problem to resolution
3. **Proactive**: Issues are automatically scheduled for resolution
4. **Real-time**: Dashboard shows live workflow status
5. **Configurable**: Settings can be adjusted based on business needs
6. **Prioritized**: Critical issues get faster response times

## **Implementation Priority**

1. **High Priority**: Auto-task creation from high/medium alerts
2. **Medium Priority**: Enhanced dashboard workflow display
3. **Low Priority**: Configuration settings and fine-tuning

This implementation completes your vision of a fully automated equipment issue resolution workflow!
