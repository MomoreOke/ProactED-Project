# Model Interpretability Integration Test Script
# Run this to test the integration after starting the application

Write-Host "🧠 Testing Model Interpretability Integration..." -ForegroundColor Cyan
Write-Host ""

# Test URLs
$baseUrl = "http://localhost:5000"  # Adjust port if needed
$testUrls = @(
    "/ModelInterpretability",
    "/Equipment",
    "/Equipment?modelInterpretability=true",
    "/ModelInterpretability/EquipmentAnalysis/1",
    "/Equipment/GetEquipmentExplanation/1"
)

# Test Model Interpretability redirect
Write-Host "1. Testing ModelInterpretability redirect..." -ForegroundColor Yellow
Write-Host "   Navigate to: $baseUrl/ModelInterpretability"
Write-Host "   Expected: Should redirect to Equipment Management with notice"
Write-Host ""

# Test Equipment Management integration
Write-Host "2. Testing Equipment Management integration..." -ForegroundColor Yellow
Write-Host "   Navigate to: $baseUrl/Equipment?modelInterpretability=true"
Write-Host "   Expected: Should show integration notice and brain icons"
Write-Host ""

# Test brain icon functionality
Write-Host "3. Testing AI Analysis buttons..." -ForegroundColor Yellow
Write-Host "   Action: Click any 🧠 brain icon in Equipment table"
Write-Host "   Expected: Modal popup with AI analysis or service connection notice"
Write-Host ""

# Test Predictive Model connection
Write-Host "4. Testing Predictive Model folder connection..." -ForegroundColor Yellow
Write-Host "   Path: C:\Users\NABILA\Desktop\ProactED-Project\Predictive Model\"
Write-Host "   Expected files:"
Write-Host "   ✓ model_interpretability.py"
Write-Host "   ✓ knust_classroom_equipment_dataset.csv"
Write-Host ""

# Test service endpoints
Write-Host "5. Testing API endpoints..." -ForegroundColor Yellow
Write-Host "   POST: $baseUrl/Equipment/GetEquipmentExplanation/{id}"
Write-Host "   GET:  $baseUrl/ModelInterpretability/CheckService"
Write-Host "   POST: $baseUrl/ModelInterpretability/StartService"
Write-Host ""

# Verification checklist
Write-Host "🔍 Integration Verification Checklist:" -ForegroundColor Green
Write-Host "┌─────────────────────────────────────────────────────────┐"
Write-Host "│ □ ModelInterpretability/Index redirects to Equipment    │"
Write-Host "│ □ Equipment page shows integration notice               │"
Write-Host "│ □ Brain icons appear next to Schedule Maintenance      │"
Write-Host "│ □ Clicking brain icon shows modal or analysis page     │"
Write-Host "│ □ Predictive Maintenance shows moved notice            │"
Write-Host "│ □ Error messages reference 5000+ dataset connection    │"
Write-Host "│ □ Documentation file created                           │"
Write-Host "└─────────────────────────────────────────────────────────┘"
Write-Host ""

Write-Host "🚀 To start testing:" -ForegroundColor Magenta
Write-Host "1. Run: dotnet run"
Write-Host "2. Navigate to http://localhost:5000/ModelInterpretability"
Write-Host "3. Verify redirect to Equipment Management"
Write-Host "4. Test brain icon functionality"
Write-Host ""

Write-Host "📁 Related Files Modified:" -ForegroundColor Blue
Write-Host "   Views/ModelInterpretability/Index.cshtml (redirect page)"
Write-Host "   Views/Equipment/Index.cshtml (brain icons added)"
Write-Host "   Views/PredictiveMaintenance/Index.cshtml (notices updated)"
Write-Host "   Controllers/ModelInterpretabilityController.cs (updated)"
Write-Host "   MODEL_INTERPRETABILITY_INTEGRATION.md (documentation)"
Write-Host ""

Write-Host "✅ Integration complete! Ready for testing." -ForegroundColor Green
