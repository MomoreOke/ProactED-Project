# Test creating a maintenance task to trigger email
Write-Host "=== Testing Maintenance Task Creation and Email ===" -ForegroundColor Green

# Test data for creating a maintenance task
$taskData = @{
    EquipmentId = 1
    Description = "Test maintenance task to verify enhanced email functionality with comprehensive equipment specifications"
    Priority = "High"
    ScheduledDate = (Get-Date).AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss")
} | ConvertTo-Json

Write-Host "Task Data:" -ForegroundColor Yellow
Write-Host $taskData

# First, let's check if the application is responsive
try {
    Write-Host "`nTesting application health..." -ForegroundColor Cyan
    $healthResponse = Invoke-WebRequest -Uri "http://localhost:5261" -Method GET -TimeoutSec 10
    Write-Host "✅ Application is responsive (Status: $($healthResponse.StatusCode))" -ForegroundColor Green
    
    # Let's try to access the maintenance scheduling page
    Write-Host "`nChecking Schedule/Create endpoint..." -ForegroundColor Cyan
    $scheduleResponse = Invoke-WebRequest -Uri "http://localhost:5261/Schedule/Create" -Method GET -TimeoutSec 10
    Write-Host "✅ Schedule page accessible (Status: $($scheduleResponse.StatusCode))" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Error accessing application: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the application is running on http://localhost:5261" -ForegroundColor Yellow
    exit 1
}

Write-Host "`n🎯 To test the enhanced email functionality:" -ForegroundColor Magenta
Write-Host "1. Open http://localhost:5261 in your browser" -ForegroundColor White
Write-Host "2. Navigate to Schedule > Create Task" -ForegroundColor White
Write-Host "3. Fill in the form with equipment details" -ForegroundColor White
Write-Host "4. The system will automatically assign a technician and send enhanced email" -ForegroundColor White
Write-Host "5. Check noahjamal303@gmail.com for the detailed equipment specifications email" -ForegroundColor White

Write-Host "`n📧 The enhanced email will include:" -ForegroundColor Cyan
Write-Host "   • Priority indicators (🚨 CRITICAL, ⚠️ HIGH PRIORITY, etc.)" -ForegroundColor Gray
Write-Host "   • Complete equipment specifications and model details" -ForegroundColor Gray
Write-Host "   • Installation date, age, and expected lifespan" -ForegroundColor Gray
Write-Host "   • Usage information and location details" -ForegroundColor Gray
Write-Host "   • Pre-maintenance checklist and safety procedures" -ForegroundColor Gray
Write-Host "   • Professional styling with organized information grids" -ForegroundColor Gray
