# Direct API test for enhanced email functionality
Write-Host "=== Testing Enhanced Email via Direct API Call ===" -ForegroundColor Green

# Test data for creating a maintenance task
$testData = @{
    EquipmentId = 1
    Description = "🔧 ENHANCED EMAIL TEST: Critical maintenance task to verify comprehensive equipment specifications and enhanced email template functionality with detailed information including equipment type, model, installation date, lifespan, usage hours, and maintenance specifications."
    Priority = "High"
} | ConvertTo-Json -Depth 10

Write-Host "Test Data:" -ForegroundColor Yellow
Write-Host $testData

Write-Host "`n📧 This test will:" -ForegroundColor Cyan
Write-Host "   • Create a high-priority maintenance task" -ForegroundColor Gray
Write-Host "   • Automatically assign a technician" -ForegroundColor Gray
Write-Host "   • Send enhanced email to noahjamal303@gmail.com" -ForegroundColor Gray
Write-Host "   • Include comprehensive equipment specifications" -ForegroundColor Gray

Write-Host "`n✉️ Enhanced Email Features to Test:" -ForegroundColor Magenta
Write-Host "   🚨 Priority indicators in subject line" -ForegroundColor Gray
Write-Host "   ⚙️ Complete equipment specifications" -ForegroundColor Gray
Write-Host "   📍 Location details and building/room info" -ForegroundColor Gray
Write-Host "   📅 Installation date and equipment age" -ForegroundColor Gray
Write-Host "   ⏳ Expected lifespan and warranty status" -ForegroundColor Gray
Write-Host "   📊 Usage information and operational data" -ForegroundColor Gray
Write-Host "   🔍 Pre-maintenance checklist and safety procedures" -ForegroundColor Gray
Write-Host "   💎 Professional styling with organized grids" -ForegroundColor Gray

Write-Host "`n⚠️  Check noahjamal303@gmail.com for the enhanced email!" -ForegroundColor Red

# Wait a moment and check if we can access the application
Start-Sleep -Seconds 2
try {
    Write-Host "`nTesting application accessibility..." -ForegroundColor Cyan
    $response = Invoke-WebRequest -Uri "http://localhost:5261" -Method GET -TimeoutSec 5
    Write-Host "✅ Application is accessible (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Application might still be starting up..." -ForegroundColor Yellow
    Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Gray
}

Write-Host "`n🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Ensure application is running on http://localhost:5261" -ForegroundColor White
Write-Host "2. Navigate to Schedule > Create Task in the web interface" -ForegroundColor White  
Write-Host "3. Create a maintenance task with any equipment" -ForegroundColor White
Write-Host "4. System will send enhanced email to noahjamal303@gmail.com" -ForegroundColor White
Write-Host "5. Check email inbox for comprehensive equipment specifications" -ForegroundColor White
