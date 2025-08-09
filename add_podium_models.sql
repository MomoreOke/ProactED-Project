-- Add sample equipment models for Podiums equipment type

-- First, find the Equipment Type ID for Podiums
DECLARE @PodiumTypeId INT;
SELECT @PodiumTypeId = EquipmentTypeId FROM EquipmentTypes WHERE EquipmentTypeName = 'Podiums';

-- If Podiums type doesn't exist, create it
IF @PodiumTypeId IS NULL
BEGIN
    INSERT INTO EquipmentTypes (EquipmentTypeName) VALUES ('Podiums');
    SET @PodiumTypeId = SCOPE_IDENTITY();
END

-- Add some sample podium models
INSERT INTO EquipmentModels (ModelName, EquipmentTypeId) 
SELECT * FROM (VALUES
    ('Standard Wooden Podium', @PodiumTypeId),
    ('Adjustable Height Podium', @PodiumTypeId),
    ('Digital Interactive Podium', @PodiumTypeId),
    ('Executive Podium with Microphone', @PodiumTypeId),
    ('Portable Folding Podium', @PodiumTypeId),
    ('Glass Top Modern Podium', @PodiumTypeId)
) AS NewModels(ModelName, EquipmentTypeId)
WHERE NOT EXISTS (
    SELECT 1 FROM EquipmentModels em 
    WHERE em.ModelName = NewModels.ModelName AND em.EquipmentTypeId = NewModels.EquipmentTypeId
);

-- Display the results
SELECT et.EquipmentTypeName, em.ModelName 
FROM EquipmentTypes et
JOIN EquipmentModels em ON et.EquipmentTypeId = em.EquipmentTypeId
WHERE et.EquipmentTypeName = 'Podiums'
ORDER BY em.ModelName;
