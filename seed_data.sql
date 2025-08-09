-- Equipment seed data for ProactED LocalDB
-- Run this script against your ProactEDDb database

-- The database already has equipment data from migrations with IDs 100+
-- We'll work with the existing equipment data

-- Add some sample maintenance logs for the existing equipment
SET IDENTITY_INSERT [MaintenanceLogs] ON;

INSERT INTO [MaintenanceLogs] ([LogId], [EquipmentId], [LogDate], [MaintenanceType], [Description], [Technician], [DowntimeDuration], [Cost], [Status])
VALUES 
(1, 100, '2024-01-15', 1, 'Routine lamp replacement', 'John Smith', '01:30:00', 150.00, 2),
(2, 101, '2024-02-20', 1, 'Filter cleaning and inspection', 'Jane Doe', '02:00:00', 75.00, 2),
(3, 105, '2024-03-10', 0, 'Software update and calibration', 'Mike Johnson', '00:45:00', 0.00, 2),
(4, 106, '2024-04-05', 2, 'Emergency bulb replacement', 'John Smith', '01:00:00', 200.00, 2),
(5, 107, '2024-05-12', 1, 'Coolant system maintenance', 'Sarah Wilson', '03:00:00', 300.00, 2);

SET IDENTITY_INSERT [MaintenanceLogs] OFF;

-- Add some alerts for existing equipment
SET IDENTITY_INSERT [Alerts] ON;

INSERT INTO [Alerts] ([AlertId], [EquipmentId], [Title], [Description], [Priority], [Status], [CreatedDate])
VALUES 
(1, 170, 'High Operating Temperature', 'Old projector showing elevated temperature readings', 1, 0, '2024-08-05'),
(2, 171, 'Unusual Vibration Detected', 'Retired AC unit showing increased vibration levels', 2, 0, '2024-08-04'),
(3, 106, 'Scheduled Maintenance Due', 'Routine maintenance window approaching for lab projector', 0, 1, '2024-08-03'),
(4, 107, 'Filter Replacement Needed', 'Air filter replacement due for lab AC unit', 1, 0, '2024-08-02'),
(5, 105, 'Calibration Required', 'Smart podium needs recalibration after recent use', 0, 1, '2024-08-01');

SET IDENTITY_INSERT [Alerts] OFF;

-- Add some failure predictions for existing equipment
SET IDENTITY_INSERT [FailurePredictions] ON;

INSERT INTO [FailurePredictions] ([PredictionId], [EquipmentId], [PredictedFailureDate], [ConfidenceLevel], [Status], [AnalysisNotes], [CreatedDate])
VALUES 
(1, 170, '2024-09-15', 85, 2, 'Age and usage patterns indicate high failure probability', '2024-08-06'),
(2, 171, '2024-10-20', 70, 1, 'Vibration analysis suggests component wear', '2024-08-06'),
(3, 100, '2025-01-10', 45, 1, 'Normal wear progression, monitoring required', '2024-08-06'),
(4, 107, '2024-11-30', 60, 1, 'Filter condition and usage history analysis', '2024-08-06'),
(5, 106, '2025-03-15', 35, 0, 'Good condition, low risk assessment', '2024-08-06');

SET IDENTITY_INSERT [FailurePredictions] OFF;

-- Add some inventory items
SET IDENTITY_INSERT [InventoryItems] ON;

INSERT INTO [InventoryItems] ([ItemId], [Name], [Description], [MinimumStockLevel], [Category], [MinStockLevel], [MaxStockLevel], [ReorderPoint], [ReorderQuantity], [CompatibleModels])
VALUES 
(1, 'Projector Lamp - Type A', 'Replacement lamp for Projector Model A', 5, 0, 5, 20, 8, 10, 'Projector Model A'),
(2, 'Projector Lamp - Type B', 'Replacement lamp for Projector Model B', 3, 0, 3, 15, 5, 8, 'Projector Model B'),
(3, 'AC Filter - Standard', 'Standard air filter for AC units', 10, 0, 10, 50, 15, 20, 'Air Conditioner Model A,Air Conditioner Model B'),
(4, 'Podium Cable Kit', 'Complete cable replacement kit for podiums', 2, 0, 2, 10, 3, 5, 'Podium Model A,Podium Model B'),
(5, 'Cleaning Supplies', 'General cleaning and maintenance supplies', 20, 1, 20, 100, 30, 50, 'All Models');

SET IDENTITY_INSERT [InventoryItems] OFF;

-- Add inventory stock
SET IDENTITY_INSERT [InventoryStocks] ON;

INSERT INTO [InventoryStocks] ([StockId], [ItemId], [Quantity], [UnitCost], [DateReceived], [BatchNumber])
VALUES 
(1, 1, 12, 45.50, '2024-07-01', 'LAMP-2024-001'),
(2, 2, 8, 52.00, '2024-07-01', 'LAMP-2024-002'),
(3, 3, 25, 15.75, '2024-07-15', 'FILT-2024-001'),
(4, 4, 5, 85.00, '2024-06-20', 'CABLE-2024-001'),
(5, 5, 45, 12.25, '2024-07-30', 'CLEAN-2024-001');

SET IDENTITY_INSERT [InventoryStocks] OFF;

PRINT 'ProactED sample data inserted successfully!';
PRINT 'Equipment Count: 72 (from migrations)';
PRINT 'Maintenance Logs: 5';
PRINT 'Alerts: 5'; 
PRINT 'Failure Predictions: 5';
PRINT 'Inventory Items: 5';
