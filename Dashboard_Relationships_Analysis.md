# 📊 Dashboard View Relationships Analysis

## 🏗️ **Application Architecture Overview**

```
┌─────────────────────────────────────────────────────────────────┐
│                        DASHBOARD HUB                             │
│                    (Central Overview)                            │
└─────────────┬─────────────┬─────────────┬─────────────┬─────────┘
             │             │             │             │
        ┌────▼────┐   ┌────▼────┐   ┌────▼────┐   ┌────▼────┐
        │Equipment│   │Maintenance│  │ Alerts  │   │Inventory│
        │Management│   │   Logs    │  │         │   │         │
        └─────────┘   └─────────┘   └─────────┘   └─────────┘
```

## 🔗 **Data Relationship Flow**

### **1. Dashboard → Equipment (Primary Relationship)**
- **Metric Displayed**: Total Equipment Count (`@Model.TotalEquipment`)
- **Navigation**: Quick Action → "Manage Equipment" → `Equipment/Index`
- **Data Source**: `_context.Equipment.CountAsync()`
- **Relationship**: One-to-Many with MaintenanceLogs, Alerts

### **2. Dashboard → Maintenance System**
- **Metrics Displayed**: 
  - Active Maintenance Tasks (`@Model.ActiveMaintenanceTasks`)
  - Upcoming Maintenance Table (`@Model.UpcomingMaintenances`)
- **Navigation**: 
  - "View All" → `MaintenanceLog/Index`
  - Quick Action → "Add Maintenance Log" → `MaintenanceLog/Create`
- **Data Source**: `_context.MaintenanceTasks` with Equipment and User relationships

### **3. Dashboard → Alert System**
- **Metrics Displayed**: Recent Alerts Table (`@Model.RecentAlerts`)
- **Navigation**: 
  - "View All" → `Alert/Index`
  - Navigation Menu → `Alert/Index`
- **Data Source**: `_context.Alerts` with Equipment and AssignedTo relationships

### **4. Dashboard → Inventory System**
- **Metrics Displayed**: Low Stock Items Count (`@Model.LowStockItems`)
- **Navigation**: Quick Action → "Check Inventory" → `Inventory/Index`
- **Data Source**: `_context.InventoryItems` with InventoryStocks relationship

### **5. Dashboard → Reporting System**
- **Navigation**: Quick Action → "View Reports" → `Report/Index`
- **Purpose**: Analytics and detailed reporting

## 📋 **View Navigation Matrix**

| From Dashboard | Target View | Relationship Type | Data Connection |
|----------------|-------------|------------------|-----------------|
| Equipment Metric | Equipment/Index | Direct Navigation | Equipment entities |
| Recent Alerts | Alert/Index | List View | Alert-Equipment-User |
| Upcoming Maintenance | MaintenanceLog/Index | List View | MaintenanceTask-Equipment |
| Quick Actions | Multiple | Direct Actions | Various entities |
| Low Stock Items | Inventory/Index | List View | InventoryItem-Stock |

## 🗄️ **Database Entity Relationships**

```
Equipment ──┬─── MaintenanceLogs
            ├─── Alerts  
            ├─── MaintenanceTasks
            └─── FailurePredictions

User ───────┬─── MaintenanceTasks (AssignedTo)
            ├─── Alerts (AssignedTo)
            └─── MaintenanceLogs (Technician)

InventoryItem ─── InventoryStock ─── MaintenanceInventoryLink ─── MaintenanceLog
```

## 🎯 **Key Interactive Elements**

### **Metric Cards (Top Row)**
1. **Total Equipment** → Equipment Management
2. **Active Tasks** → Maintenance Logs
3. **Low Stock** → Inventory Management  
4. **Active Equipment** → Equipment Status

### **Data Tables (Middle Section)**
1. **Recent Alerts Table**
   - Shows: Equipment, Priority, Date, Status
   - Links to: Alert details and full Alert list
   
2. **Upcoming Maintenance Table**
   - Shows: Equipment, Task, Due Date, Assigned User
   - Links to: Maintenance details and full Maintenance list

### **Quick Actions (Bottom Section)**
1. **Manage Equipment** → Equipment/Index
2. **Add Maintenance Log** → MaintenanceLog/Create
3. **Check Inventory** → Inventory/Index  
4. **View Reports** → Report/Index

## 🔄 **Workflow Patterns**

### **Typical User Journey 1: Equipment Issue**
```
Dashboard → Alert (equipment issue) → Equipment Details → Create Maintenance Log
```

### **Typical User Journey 2: Preventive Maintenance**
```
Dashboard → Upcoming Maintenance → Maintenance Details → Update/Complete Task
```

### **Typical User Journey 3: Inventory Management**
```
Dashboard → Low Stock Alert → Inventory → Reorder Items → Link to Maintenance
```

## 📊 **Data Aggregation Points**

### **Dashboard Controller Queries:**
1. **Equipment Count**: `Equipment.CountAsync()`
2. **Active Tasks**: `MaintenanceTasks.Where(t => t.Status != Completed)`
3. **Low Stock**: `InventoryItems.Where(stock <= minLevel)`
4. **Recent Alerts**: `Alerts.OrderByDescending(CreatedDate).Take(5)`
5. **Equipment Status**: `Equipment.GroupBy(Status)`

## 🎨 **View Design Patterns**

### **Consistent Elements Across Views:**
- **Modern Card Design**: All sections use gradient cards
- **Interactive Tables**: Hover effects and clickable rows
- **Status Badges**: Color-coded priority/status indicators
- **Navigation Buttons**: Consistent "View All" styling
- **Icon Integration**: Bootstrap icons for visual consistency

## 🔧 **Technical Implementation**

### **View Models Used:**
- `DashboardViewModel`: Aggregates all dashboard data
- Individual entity models for detailed views
- Navigation through `asp-controller` and `asp-action` tags

### **Authorization:**
- Dashboard requires `[Authorize]` attribute
- All linked views inherit authentication requirements

This dashboard serves as the **central hub** for the predictive maintenance system, providing:
- **Real-time metrics** and KPIs
- **Quick access** to all major system areas  
- **Actionable insights** through alerts and upcoming tasks
- **Efficient workflows** through direct action buttons
