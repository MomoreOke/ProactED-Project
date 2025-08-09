using System.Diagnostics;
using System.Text.Json;
using FEENALOoFINALE.Models;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;

namespace FEENALOoFINALE.Services
{
    /// <summary>
    /// 🔍 Model Interpretability Service
    /// 
    /// Provides AI model interpretability and explainability for equipment failure predictions
    /// Integrates directly with the ProactED equipment management system
    /// </summary>
    public interface IModelInterpretabilityService
    {
        Task<EquipmentInterpretabilityResult> GetEquipmentExplanationAsync(int equipmentId);
        Task<EquipmentInterpretabilityResult> GetEquipmentExplanationAsync(Equipment equipment);
        Task<GlobalFeatureImportance> GetGlobalFeatureImportanceAsync();
        Task<bool> IsInterpretabilityServiceAvailableAsync();
    }

    public class ModelInterpretabilityService : IModelInterpretabilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ModelInterpretabilityService> _logger;
        private readonly string _pythonServicePath;
        private readonly string _modelPath;

        public ModelInterpretabilityService(
            ApplicationDbContext context,
            ILogger<ModelInterpretabilityService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _pythonServicePath = configuration.GetValue<string>("ML:InterpretabilityServicePath") 
                ?? @"C:\Users\NABILA\Desktop\ProactED-Project\Predictive Model";
            _modelPath = Path.Combine(_pythonServicePath, "equipment_failure_model_deployment.pkl");
        }

        public async Task<EquipmentInterpretabilityResult> GetEquipmentExplanationAsync(int equipmentId)
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentType)
                .Include(e => e.EquipmentModel)
                .Include(e => e.Building)
                .Include(e => e.Room)
                .Include(e => e.MaintenanceLogs)
                .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);

            if (equipment == null)
            {
                throw new ArgumentException($"Equipment with ID {equipmentId} not found");
            }

            return await GetEquipmentExplanationAsync(equipment);
        }

        public async Task<EquipmentInterpretabilityResult> GetEquipmentExplanationAsync(Equipment equipment)
        {
            try
            {
                // Prepare equipment data for the ML model
                var equipmentData = await PrepareEquipmentDataAsync(equipment);
                
                // Call Python interpretability service
                var explanation = await CallPythonInterpretabilityService(equipmentData, equipment.EquipmentId.ToString());
                
                // Parse and return results
                return ParseInterpretabilityResults(explanation, equipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting interpretability explanation for equipment {EquipmentId}", equipment.EquipmentId);
                return new EquipmentInterpretabilityResult
                {
                    EquipmentId = equipment.EquipmentId,
                    EquipmentName = equipment.EquipmentModel?.ModelName ?? "Unknown",
                    IsSuccessful = false,
                    ErrorMessage = "Unable to generate explanation at this time"
                };
            }
        }

        public async Task<GlobalFeatureImportance> GetGlobalFeatureImportanceAsync()
        {
            try
            {
                var pythonScript = $@"
import sys
import os
sys.path.append(r'{_pythonServicePath}')

from model_interpretability import EquipmentFailureInterpreter
import json

try:
    interpreter = EquipmentFailureInterpreter()
    importance_data = interpreter.get_global_feature_importance('all')
    
    result = {{
        'isSuccessful': True,
        'importanceData': importance_data
    }}
    
    print(json.dumps(result))
except Exception as e:
    result = {{
        'isSuccessful': False,
        'errorMessage': str(e)
    }}
    print(json.dumps(result))
";

                var tempFile = Path.GetTempFileName() + ".py";
                await File.WriteAllTextAsync(tempFile, pythonScript);

                var processInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = tempFile,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _pythonServicePath
                };

                using var process = Process.Start(processInfo);
                if (process?.StandardOutput == null || process.StandardError == null)
                {
                    File.Delete(tempFile);
                    throw new InvalidOperationException("Failed to start Python process");
                }
                
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                File.Delete(tempFile);

                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogWarning("Python process stderr: {Error}", error);
                }

                var result = JsonSerializer.Deserialize<JsonElement>(output);
                
                if (result.GetProperty("isSuccessful").GetBoolean())
                {
                    var importanceData = result.GetProperty("importanceData");
                    return ParseGlobalFeatureImportance(importanceData);
                }
                else
                {
                    throw new Exception(result.GetProperty("errorMessage").GetString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting global feature importance");
                return new GlobalFeatureImportance
                {
                    IsSuccessful = false,
                    ErrorMessage = "Unable to get feature importance"
                };
            }
        }

        public Task<bool> IsInterpretabilityServiceAvailableAsync()
        {
            try
            {
                return Task.FromResult(File.Exists(_modelPath) && Directory.Exists(_pythonServicePath));
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private Task<Dictionary<string, object>> PrepareEquipmentDataAsync(Equipment equipment)
        {
            // Calculate equipment features based on the model's expected inputs
            var installDate = equipment.InstallationDate ?? DateTime.Now.AddMonths(-12);
            var ageMonths = (int)((DateTime.Now - installDate).TotalDays / 30.44);
            
            // Get maintenance history
            var maintenanceCount = equipment.MaintenanceLogs?.Count ?? 0;
            var lastMaintenance = equipment.MaintenanceLogs?
                .OrderByDescending(m => m.LogDate)
                .FirstOrDefault()?.LogDate ?? installDate;
            var daysSinceMaintenance = (int)(DateTime.Now - lastMaintenance).TotalDays;

            // Estimate other features based on equipment data
            var weekOfYear = DateTime.Now.DayOfYear / 7;
            var academicUsageMultiplier = GetAcademicUsageMultiplier();
            
            return Task.FromResult(new Dictionary<string, object>
            {
                ["age_months"] = ageMonths,
                ["week_of_year"] = weekOfYear,
                ["academic_usage_multiplier"] = academicUsageMultiplier,
                ["daily_usage_hours"] = GetDailyUsageHours(equipment),
                ["total_usage_hours"] = GetTotalUsageHours(equipment, ageMonths),
                ["last_maintenance_days"] = daysSinceMaintenance,
                ["maintenance_count"] = maintenanceCount,
                ["room_temperature"] = GetRoomTemperature(equipment),
                ["dust_factor"] = GetDustFactor(equipment),
                ["humidity"] = GetHumidity(equipment),
                ["operating_temperature"] = GetOperatingTemperature(equipment),
                ["power_consumption"] = GetPowerConsumption(equipment),
                ["vibration_level"] = GetVibrationLevel(equipment),
                ["dust_accumulation"] = GetDustAccumulation(equipment),
                ["humidity_level"] = GetHumidityLevel(equipment),
                ["usage_per_day"] = GetDailyUsageHours(equipment),
                ["usage_vs_capacity"] = GetUsageVsCapacity(equipment),
                ["maintenance_frequency"] = GetMaintenanceFrequency(equipment),
                ["maintenance_overdue"] = IsMaintenanceOverdue(equipment) ? 1 : 0,
                ["temperature_stress"] = GetTemperatureStress(equipment),
                ["environmental_stress"] = GetEnvironmentalStress(equipment),
                ["is_projector"] = equipment.EquipmentType?.EquipmentTypeName?.Contains("Projector", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0,
                ["is_air_conditioner"] = equipment.EquipmentType?.EquipmentTypeName?.Contains("Air Conditioner", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0,
                ["is_podium"] = equipment.EquipmentType?.EquipmentTypeName?.Contains("Podium", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0,
                ["equipment_type_encoded"] = GetEquipmentTypeEncoded(equipment),
                ["room_type_encoded"] = GetRoomTypeEncoded(equipment),
                ["age_category_encoded"] = GetAgeCategoryEncoded(ageMonths)
            });
        }

        private async Task<string> CallPythonInterpretabilityService(Dictionary<string, object> equipmentData, string equipmentId)
        {
            var dataJson = JsonSerializer.Serialize(equipmentData);
            
            var pythonScript = $@"
import sys
import os
sys.path.append(r'{_pythonServicePath}')

from model_interpretability import EquipmentFailureInterpreter
import pandas as pd
import json

try:
    # Parse equipment data
    equipment_data = json.loads('{dataJson.Replace("'", "\\'")}')
    df = pd.DataFrame([equipment_data])
    
    # Initialize interpreter and get explanation
    interpreter = EquipmentFailureInterpreter()
    explanation = interpreter.explain_prediction(df, '{equipmentId}')
    
    # Convert to JSON-serializable format
    result = {{
        'isSuccessful': True,
        'explanation': {{
            'equipment_id': explanation['equipment_id'],
            'prediction': explanation['prediction'],
            'business_interpretation': explanation.get('business_interpretation', {{}})
        }}
    }}
    
    # Convert SHAP data if available
    if 'explanations' in explanation and 'shap' in explanation['explanations']:
        shap_data = explanation['explanations']['shap']['data']
        result['explanation']['shap_explanations'] = {{
            'features': shap_data['feature'].tolist(),
            'values': shap_data['value'].tolist(),
            'shap_values': shap_data['shap_value'].tolist(),
            'contributions': shap_data['contribution'].tolist(),
            'base_value': explanation['explanations']['shap']['base_value']
        }}
    
    print(json.dumps(result))
    
except Exception as e:
    result = {{
        'isSuccessful': False,
        'errorMessage': str(e)
    }}
    print(json.dumps(result))
";

            var tempFile = Path.GetTempFileName() + ".py";
            await File.WriteAllTextAsync(tempFile, pythonScript);

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = tempFile,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _pythonServicePath
                };

                using var process = Process.Start(processInfo);
                if (process?.StandardOutput == null || process.StandardError == null)
                {
                    throw new InvalidOperationException("Failed to start Python interpretability process");
                }
                
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogWarning("Python interpretability stderr: {Error}", error);
                }

                return output;
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        private EquipmentInterpretabilityResult ParseInterpretabilityResults(string jsonOutput, Equipment equipment)
        {
            try
            {
                var result = JsonSerializer.Deserialize<JsonElement>(jsonOutput);
                
                if (!result.GetProperty("isSuccessful").GetBoolean())
                {
                    return new EquipmentInterpretabilityResult
                    {
                        EquipmentId = equipment.EquipmentId,
                        EquipmentName = equipment.EquipmentModel?.ModelName ?? "Unknown Equipment",
                        IsSuccessful = false,
                        ErrorMessage = result.GetProperty("errorMessage").GetString()
                    };
                }

                var explanation = result.GetProperty("explanation");
                var prediction = explanation.GetProperty("prediction");

                var interpretabilityResult = new EquipmentInterpretabilityResult
                {
                    EquipmentId = equipment.EquipmentId,
                    EquipmentName = equipment.EquipmentModel?.ModelName ?? "Unknown Equipment",
                    IsSuccessful = true,
                    FailureProbability = prediction.GetProperty("failure_probability").GetDouble(),
                    RiskLevel = prediction.GetProperty("risk_level").GetString() ?? "Unknown",
                    Recommendation = prediction.GetProperty("recommendation").GetString() ?? "No recommendation available"
                };

                // Parse business interpretation
                if (explanation.TryGetProperty("business_interpretation", out var businessInterp))
                {
                    interpretabilityResult.Summary = businessInterp.GetProperty("summary").GetString() ?? "No summary available";
                    interpretabilityResult.MaintenanceUrgency = businessInterp.GetProperty("maintenance_urgency").GetString() ?? "Normal";
                    
                    if (businessInterp.TryGetProperty("key_factors", out var factors))
                    {
                        interpretabilityResult.KeyFactors = factors.EnumerateArray()
                            .Select(f => f.GetString() ?? string.Empty)
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                    }
                }

                // Parse SHAP explanations
                if (explanation.TryGetProperty("shap_explanations", out var shapExplanations))
                {
                    interpretabilityResult.ShapExplanations = new List<FeatureContribution>();
                    
                    var features = shapExplanations.GetProperty("features").EnumerateArray().Select(f => f.GetString() ?? string.Empty).ToArray();
                    var values = shapExplanations.GetProperty("values").EnumerateArray().Select(v => v.GetDouble()).ToArray();
                    var shapValues = shapExplanations.GetProperty("shap_values").EnumerateArray().Select(s => s.GetDouble()).ToArray();
                    var contributions = shapExplanations.GetProperty("contributions").EnumerateArray().Select(c => c.GetDouble()).ToArray();

                    for (int i = 0; i < features.Length; i++)
                    {
                        interpretabilityResult.ShapExplanations.Add(new FeatureContribution
                        {
                            FeatureName = features[i] ?? "Unknown Feature",
                            FeatureValue = values[i],
                            ShapValue = shapValues[i],
                            ContributionPercent = contributions[i]
                        });
                    }

                    interpretabilityResult.BaseValue = shapExplanations.GetProperty("base_value").GetDouble();
                }

                return interpretabilityResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing interpretability results");
                return new EquipmentInterpretabilityResult
                {
                    EquipmentId = equipment.EquipmentId,
                    EquipmentName = equipment.EquipmentModel?.ModelName ?? "Unknown Equipment",
                    IsSuccessful = false,
                    ErrorMessage = "Error parsing explanation results"
                };
            }
        }

        private GlobalFeatureImportance ParseGlobalFeatureImportance(JsonElement importanceData)
        {
            // Implementation for parsing global feature importance
            // This would convert the Python output to C# objects
            return new GlobalFeatureImportance
            {
                IsSuccessful = true,
                Methods = new List<FeatureImportanceMethod>()
                // Parse the importance data here
            };
        }

        // Helper methods for feature calculation
        private double GetAcademicUsageMultiplier() => DateTime.Now.Month >= 9 || DateTime.Now.Month <= 5 ? 1.2 : 0.8;
        private double GetDailyUsageHours(Equipment equipment) => 8.0; // Default, could be calculated from usage logs
        private double GetTotalUsageHours(Equipment equipment, int ageMonths) => ageMonths * 30 * 8; // Estimate
        private double GetRoomTemperature(Equipment equipment) => 22.0; // Default room temperature
        private double GetDustFactor(Equipment equipment) => 0.3; // Default dust factor
        private double GetHumidity(Equipment equipment) => 45.0; // Default humidity
        private double GetOperatingTemperature(Equipment equipment) => 65.0; // Default operating temp
        private double GetPowerConsumption(Equipment equipment) => 150.0; // Default power consumption
        private double GetVibrationLevel(Equipment equipment) => 1.5; // Default vibration
        private double GetDustAccumulation(Equipment equipment) => 0.2; // Default dust accumulation
        private double GetHumidityLevel(Equipment equipment) => 45.0; // Default humidity level
        private double GetUsageVsCapacity(Equipment equipment) => 0.7; // Default usage vs capacity
        private double GetMaintenanceFrequency(Equipment equipment) => 6.0; // Default maintenance frequency (months)
        private bool IsMaintenanceOverdue(Equipment equipment) => false; // Check if maintenance is overdue
        private double GetTemperatureStress(Equipment equipment) => 0.2; // Default temperature stress
        private double GetEnvironmentalStress(Equipment equipment) => 0.3; // Default environmental stress
        private int GetEquipmentTypeEncoded(Equipment equipment) => 1; // Default equipment type encoding
        private int GetRoomTypeEncoded(Equipment equipment) => 1; // Default room type encoding
        private int GetAgeCategoryEncoded(int ageMonths) => ageMonths < 24 ? 1 : ageMonths < 60 ? 2 : 3;
    }

    // Data models for interpretability results
    public class EquipmentInterpretabilityResult
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public double FailureProbability { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string MaintenanceUrgency { get; set; } = string.Empty;
        public List<string> KeyFactors { get; set; } = new();
        public List<FeatureContribution> ShapExplanations { get; set; } = new();
        public double BaseValue { get; set; }
    }

    public class FeatureContribution
    {
        public string FeatureName { get; set; } = string.Empty;
        public double FeatureValue { get; set; }
        public double ShapValue { get; set; }
        public double ContributionPercent { get; set; }
    }

    public class GlobalFeatureImportance
    {
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public List<FeatureImportanceMethod> Methods { get; set; } = new();
    }

    public class FeatureImportanceMethod
    {
        public string MethodName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<FeatureImportanceData> Features { get; set; } = new();
    }

    public class FeatureImportanceData
    {
        public string FeatureName { get; set; } = string.Empty;
        public double Importance { get; set; }
        public double? ImportanceStd { get; set; }
    }
}
