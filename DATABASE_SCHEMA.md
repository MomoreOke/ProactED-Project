# 🗄️ Predictive Maintenance Database Schema

## Table of Contents
1. [Overview](#overview)
2. [Entity Relationship Diagram](#entity-relationship-diagram)
3. [Table Definitions](#table-definitions)
4. [Relationships](#relationships)
5. [Enums](#enums)
6. [Seeded Data](#seeded-data)
7. [Constraints](#constraints)

---

## Overview

The Predictive Maintenance system uses ASP.NET Core Identity with Entity Framework Core and SQL Server. The database schema supports equipment lifecycle management, maintenance tracking, inventory control, and predictive analytics.

**Database Provider:** SQL Server  
**ORM:** Entity Framework Core  
**Identity Framework:** ASP.NET Core Identity  

---

## Entity Relationship Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   AspNetUsers   │    │    Building     │    │ EquipmentType   │
│─────────────────│    │─────────────────│    │─────────────────│
│ Id (PK)         │    │ BuildingId (PK) │    │ EquipmentTypeId │
│ FirstName       │    │ BuildingName    │    │ EquipmentTypeName│
│ LastName        │    └─────────────────┘    └─────────────────┘
│ WorkerId        │           │                         │
│ Email           │           │ 1:M                     │ 1:M
│ UserName        │           ▼                         ▼
│ ContactNumber   │    ┌─────────────────┐    ┌─────────────────┐
│ ...Identity     │    │      Room       │    │ EquipmentModel  │
└─────────────────┘    │─────────────────│    │─────────────────│
         │              │ RoomId (PK)     │    │ EquipmentModelId│
         │ 1:M          │ RoomName        │    │ EquipmentTypeId │
         ▼              │ BuildingId (FK) │    │ ModelName       │
┌─────────────────┐    └─────────────────┘    └─────────────────┘
│      Alert      │           │                         │
│─────────────────│           │ 1:M                     │ 1:M
│ AlertId (PK)    │           ▼                         ▼
│ EquipmentId (FK)│    ┌─────────────────────────────────────────┐
│ Title           │    │              Equipment                  │
│ Description     │    │─────────────────────────────────────────│
│ Priority        │◄───┤ EquipmentId (PK)                        │
│ Status          │M:1 │ EquipmentTypeId (FK)                    │
│ AssignedToUserId│    │ EquipmentModelId (FK)                   │
└─────────────────┘    │ BuildingId (FK)                         │
                       │ RoomId (FK)                             │
┌─────────────────┐    │ InstallationDate                        │
│MaintenanceTask  │    │ ExpectedLifespanMonths                  │
│─────────────────│    │ Status                                  │
│ TaskId (PK)     │    │ Notes                                   │
│ EquipmentId (FK)│◄───┤                                         │
│ ScheduledDate   │M:1 └─────────────────────────────────────────┘
│ Status          │                        │
│ Description     │                        │ 1:M
│ AssignedToUserId│                        ▼
└─────────────────┘    ┌─────────────────────────────────────────┐
                       │           MaintenanceLog                │
┌─────────────────┐    │─────────────────────────────────────────│
│FailurePrediction│    │ LogId (PK)                              │
│─────────────────│    │ EquipmentId (FK)                        │
│ PredictionId(PK)│    │ LogDate                                 │
│ EquipmentId (FK)│◄───┤ MaintenanceType                         │
│ PredictedFailure│M:1 │ Description                             │
│ ConfidenceLevel │    │ Technician                              │
│ Status          │    │ DowntimeDuration                        │
│ AnalysisNotes   │    │ Cost                                    │
│ CreatedDate     │    │ Status                                  │
└─────────────────┘    │ AlertId (FK)                            │
                       └─────────────────────────────────────────┘
                                        │
                                        │ M:M
                                        ▼
┌─────────────────┐    ┌─────────────────────────────────────────┐
│ InventoryItem   │    │      MaintenanceInventoryLink           │
│─────────────────│    │─────────────────────────────────────────│
│ ItemId (PK)     │    │ LinkId (PK)                             │
│ Name            │◄───┤ LogId (FK)                              │
│ Description     │M:M │ ItemId (FK)                             │
│ MinStockLevel   │    │ QuantityUsed                            │
│ Category        │    └─────────────────────────────────────────┘
│ ReorderPoint    │
│ ...             │
└─────────────────┘
         │
         │ 1:M
         ▼
┌─────────────────┐
│ InventoryStock  │
│─────────────────│
│ StockId (PK)    │
│ ItemId (FK)     │
│ Quantity        │
│ UnitCost        │
│ DateReceived    │
│ BatchNumber     │
└─────────────────┘
```

---

## Table Definitions

### 🔐 AspNetUsers (Identity Framework)
```sql
AspNetUsers {
    Id: nvarchar(450) [PK]
    UserName: nvarchar(256)
    Email: nvarchar(256)
    FirstName: nvarchar(50) [Required]
    LastName: nvarchar(50) [Required]
    WorkerId: nvarchar(20) [Required, Unique]
    ContactNumber: nvarchar(max) [Phone]
    IsEmailVerified: bit [Default: 0]
    EmailVerificationToken: nvarchar(max)
    EmailVerificationTokenExpires: datetime2
    LastLogin: datetime2
    -- Standard Identity fields...
}
```

### 🏢 Building
```sql
Buildings {
    BuildingId: int [PK, Identity]
    BuildingName: nvarchar(100) [Required]
}
```

### 🚪 Room
```sql
Rooms {
    RoomId: int [PK, Identity]
    RoomName: nvarchar(max) [Required]
    BuildingId: int [FK → Buildings.BuildingId, Required]
}
```

### 🏷️ EquipmentType
```sql
EquipmentTypes {
    EquipmentTypeId: int [PK, Identity]
    EquipmentTypeName: nvarchar(max) [Required]
}
```

### 📋 EquipmentModel
```sql
EquipmentModels {
    EquipmentModelId: int [PK, Identity]
    EquipmentTypeId: int [FK → EquipmentTypes.EquipmentTypeId, Required]
    ModelName: nvarchar(max) [Required]
}
```

### ⚙️ Equipment
```sql
Equipment {
    EquipmentId: int [PK, Identity]
    EquipmentTypeId: int [FK → EquipmentTypes.EquipmentTypeId]
    EquipmentModelId: int [FK → EquipmentModels.EquipmentModelId, Required]
    BuildingId: int [FK → Buildings.BuildingId]
    RoomId: int [FK → Rooms.RoomId]
    InstallationDate: datetime2
    ExpectedLifespanMonths: int
    Status: int [Enum: Active=0, Inactive=1, Retired=2]
    Notes: nvarchar(max)
}
```

### 🔧 MaintenanceLog
```sql
MaintenanceLogs {
    LogId: int [PK, Identity]
    EquipmentId: int [FK → Equipment.EquipmentId, Required]
    LogDate: datetime2 [Required]
    MaintenanceType: int [Enum: Preventive=0, Corrective=1, Inspection=2]
    Description: nvarchar(max)
    Technician: nvarchar(max) [Required]
    DowntimeDuration: time(7)
    Cost: decimal(18,2) [Default: 0]
    Status: int [Enum: Pending=0, InProgress=1, Completed=2, Cancelled=3]
    AlertId: int [FK → Alerts.AlertId]
}
```

### 📊 FailurePrediction
```sql
FailurePredictions {
    PredictionId: int [PK, Identity]
    EquipmentId: int [FK → Equipment.EquipmentId]
    PredictedFailureDate: datetime2
    ConfidenceLevel: int [Range: 0-100]
    Status: int [Enum: Low=0, Medium=1, High=2]
    AnalysisNotes: nvarchar(max)
    ContributingFactors: nvarchar(max)
    CreatedDate: datetime2
}
```

### 🚨 Alert
```sql
Alerts {
    AlertId: int [PK, Identity]
    EquipmentId: int [FK → Equipment.EquipmentId]
    InventoryItemId: int [FK → InventoryItems.ItemId]
    Title: nvarchar(max)
    Description: nvarchar(max) [Required]
    Priority: int [Enum: Low=0, Medium=1, High=2]
    Status: int [Enum: Open=0, InProgress=1, Resolved=2, Closed=3]
    CreatedDate: datetime2
    AssignedToUserId: nvarchar(450) [FK → AspNetUsers.Id]
}
```

### 📋 MaintenanceTask
```sql
MaintenanceTasks {
    TaskId: int [PK, Identity]
    EquipmentId: int [FK → Equipment.EquipmentId, Required]
    ScheduledDate: datetime2 [Required]
    Status: int [Enum: Pending=0, InProgress=1, Completed=2, Cancelled=3]
    Description: nvarchar(max) [Required]
    AssignedToUserId: nvarchar(450) [FK → AspNetUsers.Id]
}
```

### 📦 InventoryItem
```sql
InventoryItems {
    ItemId: int [PK, Identity]
    Name: nvarchar(max) [Required]
    Description: nvarchar(max)
    MinimumStockLevel: int [Range: 0+]
    Category: int [Enum: Electrical=0, Mechanical=1, Consumable=2]
    MinStockLevel: int
    MaxStockLevel: int
    ReorderPoint: int
    ReorderQuantity: int
    CompatibleModels: nvarchar(max) [JSON]
}
```

### 📊 InventoryStock
```sql
InventoryStocks {
    StockId: int [PK, Identity]
    ItemId: int [FK → InventoryItems.ItemId, Required]
    Quantity: decimal(18,2) [Required, Range: 0.01+]
    UnitCost: decimal(18,2) [Required, Range: 0.01+]
    DateReceived: datetime2 [Required]
    BatchNumber: nvarchar(50)
}
```

### 🔗 MaintenanceInventoryLink
```sql
MaintenanceInventoryLinks {
    LinkId: int [PK, Identity]
    LogId: int [FK → MaintenanceLogs.LogId]
    ItemId: int [FK → InventoryItems.ItemId]
    QuantityUsed: int
}
```

---

## Relationships

### One-to-Many (1:M)
| Parent | Child | Foreign Key | Delete Behavior |
|--------|-------|-------------|-----------------|
| Building | Room | BuildingId | Restrict |
| Building | Equipment | BuildingId | Restrict |
| Room | Equipment | RoomId | Restrict |
| EquipmentType | EquipmentModel | EquipmentTypeId | Restrict |
| EquipmentType | Equipment | EquipmentTypeId | Restrict |
| EquipmentModel | Equipment | EquipmentModelId | Cascade |
| Equipment | MaintenanceLog | EquipmentId | Cascade |
| Equipment | FailurePrediction | EquipmentId | Cascade |
| Equipment | Alert | EquipmentId | Cascade |
| Equipment | MaintenanceTask | EquipmentId | Cascade |
| InventoryItem | InventoryStock | ItemId | Cascade |
| InventoryItem | Alert | InventoryItemId | Cascade |
| User | Alert | AssignedToUserId | Set Null |
| User | MaintenanceTask | AssignedToUserId | Set Null |

### Many-to-Many (M:M)
| Entity 1 | Entity 2 | Junction Table | Purpose |
|----------|----------|----------------|---------|
| MaintenanceLog | InventoryItem | MaintenanceInventoryLink | Track parts used in maintenance |

---

## Enums

### EquipmentStatus
```csharp
public enum EquipmentStatus
{
    Active = 0,
    Inactive = 1,
    Retired = 2
}
```

### MaintenanceType
```csharp
public enum MaintenanceType
{
    Preventive = 0,
    Corrective = 1,
    Inspection = 2
}
```

### MaintenanceStatus
```csharp
public enum MaintenanceStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}
```

### AlertPriority
```csharp
public enum AlertPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}
```

### AlertStatus
```csharp
public enum AlertStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3
}
```

### PredictionStatus
```csharp
public enum PredictionStatus
{
    Low = 0,
    Medium = 1,
    High = 2
}
```

### ItemCategory
```csharp
public enum ItemCategory
{
    Electrical = 0,
    Mechanical = 1,
    Consumable = 2
}
```

---

## Seeded Data

### Equipment Types
```sql
INSERT INTO EquipmentTypes (EquipmentTypeId, EquipmentTypeName) VALUES
(1, 'Projectors'),
(2, 'Air Conditioners'),
(3, 'Podiums');
```

### Buildings
```sql
INSERT INTO Buildings (BuildingId, BuildingName) VALUES
(1, 'Petroleum Building'),
(2, 'New Engineering Building');
```

### Rooms
```sql
INSERT INTO Rooms (RoomId, BuildingId, RoomName) VALUES
-- Petroleum Building
(1, 1, 'PB001'),
(2, 1, 'PB012'),
(3, 1, 'PB014'),
(4, 1, 'PB020'),
(5, 1, 'PB201'),
(6, 1, 'PB208'),
(7, 1, 'PB214'),
-- New Engineering Building
(8, 2, 'NEB-GF'),
(9, 2, 'NEB-FF1'),
(10, 2, 'NEB-FF2'),
(11, 2, 'NEB-SF'),
(12, 2, 'NEB-TF');
```

### Equipment Models
```sql
INSERT INTO EquipmentModels (EquipmentModelId, EquipmentTypeId, ModelName) VALUES
(1, 1, 'Projector Model A'),
(2, 1, 'Projector Model B'),
(3, 2, 'Air Conditioner Model A'),
(4, 2, 'Air Conditioner Model B'),
(5, 3, 'Podium Model A'),
(6, 3, 'Podium Model B');
```

---

## Constraints

### Primary Keys
- All tables have auto-incrementing integer primary keys except `AspNetUsers` (GUID)

### Required Fields
- **User**: FirstName, LastName, WorkerId, Email, UserName
- **Equipment**: EquipmentModelId, BuildingId, RoomId
- **MaintenanceLog**: EquipmentId, LogDate, MaintenanceType, Technician
- **InventoryStock**: ItemId, Quantity, UnitCost, DateReceived
- **Room**: BuildingId (Foreign Key constraint)

### Unique Constraints
- **User.WorkerId**: Must be unique across all users
- **User.Email**: Must be unique (Identity Framework)
- **Building.BuildingName**: Implicit unique constraint

### Check Constraints
- **InventoryStock.Quantity**: Must be > 0.01
- **InventoryStock.UnitCost**: Must be > 0.01
- **FailurePrediction.ConfidenceLevel**: Must be between 0-100

### String Length Limits
- **User.FirstName, LastName**: 50 characters
- **User.WorkerId**: 20 characters
- **Building.BuildingName**: 100 characters
- **InventoryStock.BatchNumber**: 50 characters

### Decimal Precision
- **InventoryStock.Quantity, UnitCost**: decimal(18,2)
- **MaintenanceLog.Cost**: decimal(18,2)

---

## Database Connection

**Connection String Location:** `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PredictiveMaintenanceDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Entity Framework Configuration:** `Program.cs`
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
```

---

*Generated on: July 3, 2025*  
*Database Schema Version: 1.0*  
*Last Migration: 20250611182533_FinishingTouches*
