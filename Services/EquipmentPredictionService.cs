using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using FEENALOoFINALE.Hubs;

namespace FEENALOoFINALE.Services
{
    public interface IEquipmentPredictionService
    {
        Task<PredictionResult> PredictEquipmentFailureAsync(EquipmentPredictionData data);
        Task<BatchPredictionResult> PredictBatchEquipmentFailureAsync(List<EquipmentPredictionData> equipmentList);
        Task<bool> IsApiHealthyAsync();
        Task<ModelInfo> GetModelInfoAsync();
        Task<EnhancedPredictionResult> PredictWithMaintenanceSchedulingAsync(int equipmentId);
        Task<BatchEnhancedPredictionResult> PredictBatchWithMaintenanceSchedulingAsync(List<int> equipmentIds);
        Task<MaintenanceScheduleRecommendation> GetMaintenanceRecommendationAsync(int equipmentId, PredictionResult prediction);
    }

    public class EquipmentPredictionService : IEquipmentPredictionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly ILogger<EquipmentPredictionService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly MaintenanceSchedulingService _schedulingService;
        private readonly IHubContext<MaintenanceHub> _hubContext;

        public EquipmentPredictionService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            ILogger<EquipmentPredictionService> logger,
            ApplicationDbContext context,
            MaintenanceSchedulingService schedulingService,
            IHubContext<MaintenanceHub> hubContext)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["PredictionApi:BaseUrl"] ?? "http://localhost:5000";
            _logger = logger;
            _context = context;
            _schedulingService = schedulingService;
            _hubContext = hubContext;
            
            // Configure timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<PredictionResult> PredictEquipmentFailureAsync(EquipmentPredictionData data)
        {
            try
            {
                _logger.LogInformation("Starting prediction for equipment {EquipmentId} using API: {ApiUrl}", 
                    data.EquipmentId, _apiBaseUrl);

                var requestData = new
                {
                    equipment_id = data.EquipmentId,
                    age_months = data.AgeMonths,
                    operating_temperature = data.OperatingTemperature,
                    vibration_level = data.VibrationLevel,
                    power_consumption = data.PowerConsumption
                };

                var json = JsonSerializer.Serialize(requestData);
                _logger.LogDebug("Request payload: {Json}", json);
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/equipment/predict", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("API Response: {ResponseContent}", responseContent);
                    
                    var apiResponse = JsonSerializer.Deserialize<ApiPredictionResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Successfully received prediction for {EquipmentId}: Risk={RiskLevel}, Probability={Probability}", 
                        data.EquipmentId, apiResponse?.RiskLevel, apiResponse?.FailureProbability);

                    return new PredictionResult
                    {
                        Success = apiResponse?.Success ?? false,
                        EquipmentId = apiResponse?.EquipmentId ?? data.EquipmentId,
                        FailureProbability = apiResponse?.FailureProbability ?? 0.0,
                        RiskLevel = apiResponse?.RiskLevel ?? "Unknown",
                        ConfidenceScore = apiResponse?.ConfidenceScore ?? 0.0,
                        PredictionTimestamp = apiResponse?.PredictionTimestamp ?? DateTime.Now,
                        ModelVersion = apiResponse?.ModelVersion ?? "Unknown",
                        ErrorMessage = null
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Prediction API returned error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    
                    return new PredictionResult
                    {
                        Success = false,
                        EquipmentId = data.EquipmentId,
                        ErrorMessage = $"API Error: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error when calling prediction API for equipment {EquipmentId}", data.EquipmentId);
                return new PredictionResult
                {
                    Success = false,
                    EquipmentId = data.EquipmentId,
                    ErrorMessage = "Network error: Unable to connect to prediction service"
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout when calling prediction API for equipment {EquipmentId}", data.EquipmentId);
                return new PredictionResult
                {
                    Success = false,
                    EquipmentId = data.EquipmentId,
                    ErrorMessage = "Timeout: Prediction service took too long to respond"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when calling prediction API for equipment {EquipmentId}", data.EquipmentId);
                return new PredictionResult
                {
                    Success = false,
                    EquipmentId = data.EquipmentId,
                    ErrorMessage = "Unexpected error occurred"
                };
            }
        }

        public async Task<BatchPredictionResult> PredictBatchEquipmentFailureAsync(List<EquipmentPredictionData> equipmentList)
        {
            try
            {
                var requestData = new
                {
                    equipment_list = equipmentList.Select(e => new
                    {
                        equipment_id = e.EquipmentId,
                        age_months = e.AgeMonths,
                        operating_temperature = e.OperatingTemperature,
                        vibration_level = e.VibrationLevel,
                        power_consumption = e.PowerConsumption
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/equipment/batch-predict", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiBatchPredictionResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new BatchPredictionResult
                    {
                        Success = apiResponse?.Success ?? false,
                        ProcessedCount = apiResponse?.ProcessedCount ?? 0,
                        Predictions = apiResponse?.Predictions?.Select(p => new PredictionResult
                        {
                            Success = true,
                            EquipmentId = p.EquipmentId,
                            FailureProbability = p.FailureProbability,
                            RiskLevel = p.RiskLevel,
                            ConfidenceScore = p.ConfidenceScore,
                            PredictionTimestamp = p.PredictionTimestamp,
                            ModelVersion = p.ModelVersion
                        }).ToList() ?? new List<PredictionResult>(),
                        ErrorMessage = null
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Batch prediction API returned error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    
                    return new BatchPredictionResult
                    {
                        Success = false,
                        ProcessedCount = 0,
                        Predictions = new List<PredictionResult>(),
                        ErrorMessage = $"API Error: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch prediction for {Count} equipment items", equipmentList.Count);
                return new BatchPredictionResult
                {
                    Success = false,
                    ProcessedCount = 0,
                    Predictions = new List<PredictionResult>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> IsApiHealthyAsync()
        {
            try
            {
                _logger.LogInformation("Checking API health at: {ApiUrl}", $"{_apiBaseUrl}/api/health");
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/health");
                var isHealthy = response.IsSuccessStatusCode;
                
                if (isHealthy)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API is healthy. Response: {Response}", content);
                }
                else
                {
                    _logger.LogWarning("API health check failed. Status: {StatusCode}", response.StatusCode);
                }
                
                return isHealthy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API health check failed with exception");
                return false;
            }
        }

        public async Task<ModelInfo> GetModelInfoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/model/info");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiModelInfoResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Parse the training date from string if needed
                    DateTime trainingDate = DateTime.MinValue;
                    if (apiResponse != null)
                    {
                        if (apiResponse.TrainingDate != DateTime.MinValue)
                        {
                            trainingDate = apiResponse.TrainingDate;
                        }
                        else if (!string.IsNullOrEmpty(apiResponse.Training_Date))
                        {
                            if (DateTime.TryParse(apiResponse.Training_Date, out var parsedDate))
                            {
                                trainingDate = parsedDate;
                            }
                        }
                    }

                    return new ModelInfo
                    {
                        Success = apiResponse?.Success ?? false,
                        ModelVersion = apiResponse?.ModelVersion ?? "Unknown",
                        TrainingDate = trainingDate != DateTime.MinValue ? trainingDate : DateTime.Today,
                        Accuracy = apiResponse?.R2_Score ?? apiResponse?.Accuracy ?? 0.0,
                        Features = apiResponse?.Features ?? new List<string>()
                    };
                }
                
                return new ModelInfo { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model info");
                return new ModelInfo { Success = false };
            }
        }

        /// <summary>
        /// Enhanced prediction with automatic maintenance scheduling
        /// </summary>
        public async Task<EnhancedPredictionResult> PredictWithMaintenanceSchedulingAsync(int equipmentId)
        {
            try
            {
                // Get equipment with related data
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Include(e => e.Building)
                    .Include(e => e.Room)
                    .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);

                if (equipment == null)
                {
                    return new EnhancedPredictionResult
                    {
                        Success = false,
                        EquipmentId = equipmentId.ToString(),
                        ErrorMessage = "Equipment not found"
                    };
                }

                // Get ML prediction
                var predictionData = EquipmentPredictionData.FromEquipment(equipment);
                var prediction = await PredictEquipmentFailureAsync(predictionData);

                if (!prediction.Success)
                {
                    return new EnhancedPredictionResult
                    {
                        Success = false,
                        EquipmentId = equipmentId.ToString(),
                        ErrorMessage = prediction.ErrorMessage
                    };
                }

                // Analyze potential failure causes
                var failureCauses = AnalyzePotentialFailureCauses(equipment, prediction);

                // Calculate maintenance recommendations
                var maintenanceRecommendation = await GetMaintenanceRecommendationAsync(equipmentId, prediction);

                // Auto-schedule maintenance if risk is high or critical
                MaintenanceTask? scheduledTask = null;
                if (prediction.FailureProbability >= 0.6) // High or Critical risk
                {
                    scheduledTask = await AutoSchedulePreventiveMaintenance(equipment, prediction, failureCauses);
                    
                    // Notify via SignalR
                    await _hubContext.Clients.All.SendAsync("HighRiskEquipmentDetected", new
                    {
                        EquipmentId = equipmentId,
                        EquipmentName = $"{equipment.EquipmentType?.EquipmentTypeName} - {equipment.EquipmentModel?.ModelName}",
                        Location = $"{equipment.Building?.BuildingName} - {equipment.Room?.RoomName}",
                        RiskLevel = prediction.RiskLevel,
                        FailureProbability = prediction.FailureProbability,
                        MaintenanceScheduled = scheduledTask != null,
                        ScheduledDate = scheduledTask?.ScheduledDate
                    });
                }

                // Create enhanced result
                var result = new EnhancedPredictionResult
                {
                    Success = true,
                    EquipmentId = equipmentId.ToString(),
                    EquipmentName = $"{equipment.EquipmentType?.EquipmentTypeName} - {equipment.EquipmentModel?.ModelName}",
                    Location = $"{equipment.Building?.BuildingName} - {equipment.Room?.RoomName}",
                    FailureProbability = prediction.FailureProbability,
                    RiskLevel = prediction.RiskLevel,
                    ConfidenceScore = prediction.ConfidenceScore,
                    PredictionTimestamp = prediction.PredictionTimestamp,
                    ModelVersion = prediction.ModelVersion,
                    PotentialFailureCauses = failureCauses,
                    MaintenanceRecommendation = maintenanceRecommendation,
                    ScheduledMaintenanceTask = scheduledTask
                };

                // Save prediction to database
                await SavePredictionToDatabase(equipment, prediction, failureCauses);

                _logger.LogInformation("Enhanced prediction completed for equipment {EquipmentId}: Risk={RiskLevel}, Maintenance Scheduled={MaintenanceScheduled}", 
                    equipmentId, prediction.RiskLevel, scheduledTask != null);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in enhanced prediction for equipment {EquipmentId}", equipmentId);
                return new EnhancedPredictionResult
                {
                    Success = false,
                    EquipmentId = equipmentId.ToString(),
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Batch enhanced prediction with maintenance scheduling
        /// </summary>
        public async Task<BatchEnhancedPredictionResult> PredictBatchWithMaintenanceSchedulingAsync(List<int> equipmentIds)
        {
            var results = new List<EnhancedPredictionResult>();
            var totalScheduledTasks = 0;

            foreach (var equipmentId in equipmentIds)
            {
                var result = await PredictWithMaintenanceSchedulingAsync(equipmentId);
                results.Add(result);
                
                if (result.ScheduledMaintenanceTask != null)
                    totalScheduledTasks++;
            }

            return new BatchEnhancedPredictionResult
            {
                Success = true,
                ProcessedCount = equipmentIds.Count,
                EnhancedPredictions = results,
                TotalMaintenanceTasksScheduled = totalScheduledTasks
            };
        }

        /// <summary>
        /// Get maintenance recommendation based on prediction
        /// </summary>
        public async Task<MaintenanceScheduleRecommendation> GetMaintenanceRecommendationAsync(int equipmentId, PredictionResult prediction)
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentType)
                .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);

            if (equipment == null)
            {
                return new MaintenanceScheduleRecommendation
                {
                    RecommendedDate = DateTime.Now.AddDays(30),
                    MaintenanceType = "General",
                    Priority = TaskPriority.Medium,
                    Description = "Equipment not found"
                };
            }

            // Calculate days until predicted failure
            var daysUntilFailure = prediction.FailureProbability switch
            {
                >= 0.8 => 7,    // Critical: 1 week
                >= 0.6 => 30,   // High: 1 month  
                >= 0.4 => 90,   // Medium: 3 months
                >= 0.2 => 180,  // Low-Medium: 6 months
                _ => 365        // Low: 1 year
            };

            // Schedule maintenance 5-7 days before predicted failure for high-risk equipment
            var maintenanceBuffer = prediction.FailureProbability >= 0.6 ? 
                Math.Min(daysUntilFailure - 5, daysUntilFailure * 0.8) : // 5 days early or 20% buffer
                daysUntilFailure * 0.5; // 50% buffer for lower risk

            maintenanceBuffer = Math.Max(maintenanceBuffer, 1); // At least 1 day buffer

            var recommendedDate = DateTime.Now.AddDays(Math.Max(1, daysUntilFailure - maintenanceBuffer));

            // Determine maintenance type and priority
            var (maintenanceType, priority, description) = GetMaintenanceTypeAndPriority(equipment, prediction);

            return new MaintenanceScheduleRecommendation
            {
                RecommendedDate = recommendedDate,
                MaintenanceType = maintenanceType,
                Priority = priority,
                Description = description,
                DaysUntilRecommended = (int)(recommendedDate - DateTime.Now).TotalDays,
                EstimatedFailureDate = DateTime.Now.AddDays(daysUntilFailure),
                PreventiveAction = true
            };
        }

        /// <summary>
        /// Analyze potential failure causes based on equipment data and prediction
        /// </summary>
        private List<string> AnalyzePotentialFailureCauses(Equipment equipment, PredictionResult prediction)
        {
            var causes = new List<string>();
            var predictionData = EquipmentPredictionData.FromEquipment(equipment);

            // Age-related causes
            var ageMonths = equipment.InstallationDate.HasValue 
                ? (int)(DateTime.Now - equipment.InstallationDate.Value).TotalDays / 30
                : 12;

            if (ageMonths > 60) // 5+ years
                causes.Add($"Equipment age ({ageMonths / 12:F1} years) - increased wear and component degradation expected");
            else if (ageMonths > 36) // 3+ years
                causes.Add($"Equipment age ({ageMonths / 12:F1} years) - moderate wear patterns developing");

            // Temperature-related causes
            if (predictionData.OperatingTemperature > 70)
                causes.Add($"High operating temperature ({predictionData.OperatingTemperature:F1}°C) - thermal stress on components");
            else if (predictionData.OperatingTemperature > 55)
                causes.Add($"Elevated operating temperature ({predictionData.OperatingTemperature:F1}°C) - potential cooling system issues");

            // Vibration-related causes
            if (predictionData.VibrationLevel > 3.0)
                causes.Add($"High vibration levels ({predictionData.VibrationLevel:F1}) - mechanical stress and potential mounting issues");
            else if (predictionData.VibrationLevel > 2.0)
                causes.Add($"Elevated vibration levels ({predictionData.VibrationLevel:F1}) - mechanical wear acceleration");

            // Power consumption causes
            var expectedPower = GetExpectedPowerConsumption(equipment);
            var powerVariance = Math.Abs(predictionData.PowerConsumption - expectedPower) / expectedPower;
            
            if (powerVariance > 0.3) // 30% variance
            {
                if (predictionData.PowerConsumption > expectedPower)
                    causes.Add($"Higher than expected power consumption ({predictionData.PowerConsumption:F0}W vs expected {expectedPower:F0}W) - potential component inefficiency");
                else
                    causes.Add($"Lower than expected power consumption ({predictionData.PowerConsumption:F0}W vs expected {expectedPower:F0}W) - possible component degradation");
            }

            // Usage pattern causes
            if (equipment.AverageWeeklyUsageHours > 60)
                causes.Add($"High usage pattern ({equipment.AverageWeeklyUsageHours:F1} hours/week) - accelerated wear from intensive use");
            
            // Recent maintenance history
            var recentMaintenanceCount = _context.MaintenanceLogs
                .Where(ml => ml.EquipmentId == equipment.EquipmentId && 
                            ml.LogDate >= DateTime.Now.AddDays(-90))
                .Count();

            if (recentMaintenanceCount > 2)
                causes.Add("Frequent recent maintenance issues - indicating developing systematic problems");

            // Equipment type specific causes
            var equipmentType = equipment.EquipmentType?.EquipmentTypeName?.ToLower() ?? "";
            var typeSpecificCause = GetEquipmentTypeSpecificCauses(equipmentType, predictionData);
            if (!string.IsNullOrEmpty(typeSpecificCause))
                causes.Add(typeSpecificCause);

            // If no specific causes identified, provide general assessment
            if (!causes.Any())
            {
                if (prediction.FailureProbability >= 0.6)
                    causes.Add("Multiple operational parameters indicate potential system stress - comprehensive inspection recommended");
                else
                    causes.Add("Normal operational parameters detected - routine maintenance sufficient");
            }

            return causes;
        }

        /// <summary>
        /// Get equipment type specific failure causes
        /// </summary>
        private string GetEquipmentTypeSpecificCauses(string equipmentType, EquipmentPredictionData data)
        {
            return equipmentType switch
            {
                var type when type.Contains("projector") => 
                    data.OperatingTemperature > 65 ? "Projector lamp overheating - filter cleaning or lamp replacement may be needed" :
                    data.VibrationLevel > 1.5 ? "Projector cooling fan issues - potential bearing wear or dust accumulation" : "",
                    
                var type when type.Contains("computer") => 
                    data.OperatingTemperature > 50 ? "Computer thermal management issues - CPU/GPU thermal paste degradation or fan failure" :
                    data.PowerConsumption > 200 ? "Computer power supply inefficiency - possible capacitor degradation" : "",
                    
                var type when type.Contains("air condition") => 
                    data.VibrationLevel > 4.0 ? "AC compressor mechanical stress - refrigerant issues or bearing wear" :
                    data.PowerConsumption > 2000 ? "AC system inefficiency - possible refrigerant leaks or compressor degradation" : "",
                    
                var type when type.Contains("printer") => 
                    data.VibrationLevel > 2.5 ? "Printer mechanical components wear - paper feed mechanism or print head issues" :
                    data.PowerConsumption > 300 ? "Printer fuser unit inefficiency - heating element degradation" : "",
                    
                _ => ""
            };
        }

        /// <summary>
        /// Get expected power consumption for equipment type
        /// </summary>
        private double GetExpectedPowerConsumption(Equipment equipment)
        {
            var equipmentType = equipment.EquipmentType?.EquipmentTypeName?.ToLower() ?? "";
            return equipmentType switch
            {
                var type when type.Contains("projector") => 250.0,
                var type when type.Contains("computer") => 150.0,
                var type when type.Contains("air condition") => 1500.0,
                var type when type.Contains("printer") => 200.0,
                _ => 300.0
            };
        }

        /// <summary>
        /// Determine maintenance type and priority based on equipment and prediction
        /// </summary>
        private (string maintenanceType, TaskPriority priority, string description) GetMaintenanceTypeAndPriority(
            Equipment equipment, PredictionResult prediction)
        {
            var equipmentType = equipment.EquipmentType?.EquipmentTypeName?.ToLower() ?? "";
            var priority = prediction.FailureProbability switch
            {
                >= 0.8 => TaskPriority.Critical,
                >= 0.6 => TaskPriority.High,
                >= 0.4 => TaskPriority.Medium,
                _ => TaskPriority.Low
            };

            var maintenanceType = prediction.FailureProbability >= 0.6 ? "Emergency Preventive" : "Routine Preventive";
            
            var description = equipmentType switch
            {
                var type when type.Contains("projector") => 
                    $"Projector maintenance: Clean air filters, check lamp hours, inspect cooling system. Risk level: {prediction.RiskLevel}",
                var type when type.Contains("computer") => 
                    $"Computer maintenance: Clean internals, check thermal paste, update software, test hardware. Risk level: {prediction.RiskLevel}",
                var type when type.Contains("air condition") => 
                    $"AC maintenance: Check refrigerant levels, clean coils, inspect belts and bearings. Risk level: {prediction.RiskLevel}",
                var type when type.Contains("printer") => 
                    $"Printer maintenance: Clean print heads, check paper feed mechanism, replace consumables. Risk level: {prediction.RiskLevel}",
                _ => $"General equipment maintenance and inspection required. Risk level: {prediction.RiskLevel}"
            };

            return (maintenanceType, priority, description);
        }

        /// <summary>
        /// Automatically schedule preventive maintenance for high-risk equipment
        /// </summary>
        private async Task<MaintenanceTask?> AutoSchedulePreventiveMaintenance(
            Equipment equipment, PredictionResult prediction, List<string> failureCauses)
        {
            try
            {
                // Check if there's already a pending maintenance task
                var existingTask = await _context.MaintenanceTasks
                    .Where(mt => mt.EquipmentId == equipment.EquipmentId && 
                                (mt.Status == MaintenanceStatus.Pending || mt.Status == MaintenanceStatus.InProgress))
                    .FirstOrDefaultAsync();

                if (existingTask != null)
                {
                    _logger.LogInformation("Maintenance task already exists for equipment {EquipmentId}, skipping auto-scheduling", equipment.EquipmentId);
                    return existingTask;
                }

                // Get maintenance recommendation
                var recommendation = await GetMaintenanceRecommendationAsync(equipment.EquipmentId, prediction);

                // Create detailed description with failure causes
                var description = $"{recommendation.Description}\n\nPredicted Failure Risk: {prediction.FailureProbability:P1} ({prediction.RiskLevel})\n\nPotential Causes:\n" +
                    string.Join("\n", failureCauses.Select(c => $"• {c}"));

                // Create maintenance task using the scheduling service
                var maintenanceTask = await _schedulingService.CreateMaintenanceTaskAsync(
                    equipment.EquipmentId,
                    description,
                    recommendation.Priority
                );

                // Override the scheduled date with our recommendation
                maintenanceTask.ScheduledDate = recommendation.RecommendedDate;

                // Save the task
                _context.MaintenanceTasks.Add(maintenanceTask);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Auto-scheduled preventive maintenance for equipment {EquipmentId} on {ScheduledDate} (Priority: {Priority})", 
                    equipment.EquipmentId, recommendation.RecommendedDate, recommendation.Priority);

                return maintenanceTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-scheduling maintenance for equipment {EquipmentId}", equipment.EquipmentId);
                return null;
            }
        }

        /// <summary>
        /// Save prediction results to database
        /// </summary>
        private async Task SavePredictionToDatabase(Equipment equipment, PredictionResult prediction, List<string> failureCauses)
        {
            try
            {
                var failurePrediction = new FailurePrediction
                {
                    EquipmentId = equipment.EquipmentId,
                    PredictedFailureDate = prediction.CalculateFailureDate(),
                    ConfidenceLevel = (int)(prediction.ConfidenceScore * 100),
                    Status = prediction.RiskLevel.ToLower() switch
                    {
                        "critical" or "high" => PredictionStatus.High,
                        "medium" => PredictionStatus.Medium,
                        _ => PredictionStatus.Low
                    },
                    AnalysisNotes = $"ML Prediction - Model: {prediction.ModelVersion}, Risk: {prediction.RiskLevel}, Probability: {prediction.FailureProbability:P2}",
                    ContributingFactors = string.Join("; ", failureCauses),
                    CreatedDate = prediction.PredictionTimestamp
                };

                _context.FailurePredictions.Add(failurePrediction);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving prediction to database for equipment {EquipmentId}", equipment.EquipmentId);
            }
        }
    }

    // API Response DTOs
    public class ApiPredictionResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("equipment_id")]
        public string EquipmentId { get; set; } = "";
        
        [JsonPropertyName("failure_probability")]
        public double FailureProbability { get; set; }
        
        [JsonPropertyName("risk_level")]
        public string RiskLevel { get; set; } = "";
        
        [JsonPropertyName("confidence_score")]
        public double ConfidenceScore { get; set; }
        
        [JsonPropertyName("prediction_timestamp")]
        public DateTime PredictionTimestamp { get; set; }
        
        [JsonPropertyName("model_version")]
        public string ModelVersion { get; set; } = "";
    }

    public class ApiBatchPredictionResponse
    {
        public bool Success { get; set; }
        public int ProcessedCount { get; set; }
        public List<ApiPredictionResponse> Predictions { get; set; } = new();
    }

    public class ApiModelInfoResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("model_version")]
        public string ModelVersion { get; set; } = "";
        
        [JsonPropertyName("training_date")]
        public string Training_Date { get; set; } = "";
        
        public DateTime TrainingDate { get; set; }
        
        [JsonPropertyName("accuracy")]
        public double Accuracy { get; set; }
        
        [JsonPropertyName("r2_score")]
        public double R2_Score { get; set; }
        
        [JsonPropertyName("features")]
        public List<string> Features { get; set; } = new();
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
        
        [JsonPropertyName("note")]
        public string Note { get; set; } = "";
    }

    /// <summary>
    /// Enhanced prediction result with maintenance scheduling and failure cause analysis
    /// </summary>
    public class EnhancedPredictionResult
    {
        public bool Success { get; set; }
        public string EquipmentId { get; set; } = "";
        public string EquipmentName { get; set; } = "";
        public string Location { get; set; } = "";
        public double FailureProbability { get; set; }
        public string RiskLevel { get; set; } = "";
        public double ConfidenceScore { get; set; }
        public DateTime PredictionTimestamp { get; set; }
        public string ModelVersion { get; set; } = "";
        public List<string> PotentialFailureCauses { get; set; } = new();
        public MaintenanceScheduleRecommendation MaintenanceRecommendation { get; set; } = new();
        public MaintenanceTask? ScheduledMaintenanceTask { get; set; }
        public string? ErrorMessage { get; set; }

        // Computed properties for display
        public string FailureProbabilityDisplay => FailureProbability.ToString("P1");
        public string ConfidenceDisplay => ConfidenceScore.ToString("P1");
        public string RiskLevelClass => RiskLevel switch
        {
            "Critical" or "High" => "danger",
            "Medium" => "warning",
            "Low" => "success",
            _ => "secondary"
        };
        public bool MaintenanceScheduled => ScheduledMaintenanceTask != null;
        public string StatusDisplay => Success ? "Prediction Successful" : $"Error: {ErrorMessage}";
    }

    /// <summary>
    /// Maintenance schedule recommendation based on ML prediction
    /// </summary>
    public class MaintenanceScheduleRecommendation
    {
        public DateTime RecommendedDate { get; set; }
        public string MaintenanceType { get; set; } = "";
        public TaskPriority Priority { get; set; }
        public string Description { get; set; } = "";
        public int DaysUntilRecommended { get; set; }
        public DateTime EstimatedFailureDate { get; set; }
        public bool PreventiveAction { get; set; }

        // Computed properties
        public string PriorityDisplay => Priority.ToString();
        public string PriorityClass => Priority switch
        {
            TaskPriority.Critical => "danger",
            TaskPriority.High => "warning", 
            TaskPriority.Medium => "info",
            TaskPriority.Low => "success"
        };
        public string UrgencyDisplay => DaysUntilRecommended switch
        {
            <= 1 => "Immediate",
            <= 7 => "This Week",
            <= 30 => "This Month",
            _ => "Scheduled"
        };
        public int DaysUntilFailure => (int)(EstimatedFailureDate - DateTime.Now).TotalDays;
        public string MaintenanceBufferInfo => $"Maintenance scheduled {DaysUntilFailure - DaysUntilRecommended} days before predicted failure";
    }

    /// <summary>
    /// Batch result for enhanced predictions
    /// </summary>
    public class BatchEnhancedPredictionResult
    {
        public bool Success { get; set; }
        public int ProcessedCount { get; set; }
        public List<EnhancedPredictionResult> EnhancedPredictions { get; set; } = new();
        public int TotalMaintenanceTasksScheduled { get; set; }
        public string? ErrorMessage { get; set; }

        // Summary statistics
        public int HighRiskCount => EnhancedPredictions.Count(p => p.RiskLevel == "Critical" || p.RiskLevel == "High");
        public int MediumRiskCount => EnhancedPredictions.Count(p => p.RiskLevel == "Medium");
        public int LowRiskCount => EnhancedPredictions.Count(p => p.RiskLevel == "Low");
        public double AverageFailureProbability => EnhancedPredictions.Any() ? 
            EnhancedPredictions.Where(p => p.Success).Average(p => p.FailureProbability) : 0.0;
        public int SuccessfulPredictions => EnhancedPredictions.Count(p => p.Success);
        public double SuccessRate => ProcessedCount > 0 ? (double)SuccessfulPredictions / ProcessedCount : 0.0;
    }
}

// Extension method for PredictionResult
public static class PredictionResultExtensions
{
    /// <summary>
    /// Calculate estimated failure date based on probability
    /// </summary>
    public static DateTime CalculateFailureDate(this PredictionResult prediction)
    {
        var daysUntilFailure = prediction.FailureProbability switch
        {
            >= 0.8 => 7,    // Critical: 1 week
            >= 0.6 => 30,   // High: 1 month
            >= 0.4 => 90,   // Medium: 3 months
            >= 0.2 => 180,  // Low-Medium: 6 months
            _ => 365        // Low: 1 year
        };

        return DateTime.Now.AddDays(daysUntilFailure);
    }
}
