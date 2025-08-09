# Equipment Button Functionality Diagnostic Script
# This script helps diagnose issues with "Add New" and Delete button functionality

Write-Host "🔧 ProactED Equipment Button Diagnostics" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET application is running
Write-Host "1. Checking if application is running..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5261" -TimeoutSec 5 -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✅ Application is running on localhost:5261" -ForegroundColor Green
    }
} catch {
    Write-Host "   ❌ Application not running on localhost:5261" -ForegroundColor Red
    Write-Host "   Starting application..." -ForegroundColor Yellow
    Start-Process powershell -ArgumentList "cd 'C:\Users\NABILA\Desktop\ProactED-Project'; dotnet run" -WindowStyle Minimized
    Start-Sleep 10
}

# Test Equipment controller endpoint
Write-Host "2. Testing Equipment controller endpoints..." -ForegroundColor Yellow
$endpoints = @(
    "http://localhost:5261/Equipment",
    "http://localhost:5261/Equipment/Create",
    "http://localhost:5261/Equipment/CreateEquipmentType"
)

foreach ($endpoint in $endpoints) {
    try {
        $response = Invoke-WebRequest -Uri $endpoint -TimeoutSec 5 -UseBasicParsing
        $status = if ($response.StatusCode -eq 200) { "✅" } else { "⚠️" }
        Write-Host "   $status $endpoint - Status: $($response.StatusCode)" -ForegroundColor $(if ($response.StatusCode -eq 200) { "Green" } else { "Yellow" })
    } catch {
        Write-Host "   ❌ $endpoint - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "3. Opening diagnostic tools..." -ForegroundColor Yellow

# Open the diagnostic page
$diagnosticPath = "C:\Users\NABILA\Desktop\ProactED-Project\button_diagnostic.html"
Write-Host "   📋 Opening Button Diagnostic Page..." -ForegroundColor Cyan
Start-Process $diagnosticPath

# Open the actual Equipment Create page
Write-Host "   🔧 Opening Equipment Create Page..." -ForegroundColor Cyan
Start-Sleep 2
Start-Process "http://localhost:5261/Equipment/Create"

# Open an Equipment Details page (if any equipment exists)
Write-Host "   📱 Opening Equipment List..." -ForegroundColor Cyan
Start-Sleep 2
Start-Process "http://localhost:5261/Equipment"

Write-Host ""
Write-Host "🎯 TESTING INSTRUCTIONS:" -ForegroundColor White -BackgroundColor DarkBlue
Write-Host "========================" -ForegroundColor White -BackgroundColor DarkBlue
Write-Host ""
Write-Host "DIAGNOSTIC PAGE TESTS:" -ForegroundColor Cyan
Write-Host "1. Test 'Add New' button functionality in the diagnostic page" -ForegroundColor White
Write-Host "2. Test Delete button functionality in the diagnostic page" -ForegroundColor White
Write-Host "3. Test AJAX connectivity" -ForegroundColor White
Write-Host ""
Write-Host "ACTUAL APPLICATION TESTS:" -ForegroundColor Yellow
Write-Host "1. Go to Equipment Create page (should be open)" -ForegroundColor White
Write-Host "2. Try clicking 'Add New' buttons next to dropdowns" -ForegroundColor White
Write-Host "3. Go to Equipment Details page and try Delete button" -ForegroundColor White
Write-Host ""
Write-Host "BROWSER CONSOLE DEBUGGING:" -ForegroundColor Red
Write-Host "1. Press F12 to open developer tools" -ForegroundColor White
Write-Host "2. Go to Console tab" -ForegroundColor White
Write-Host "3. Look for JavaScript errors when clicking buttons" -ForegroundColor White
Write-Host "4. Check Network tab for failed AJAX requests" -ForegroundColor White
Write-Host ""
Write-Host "📝 REPORT BACK:" -ForegroundColor Magenta
Write-Host "- Which buttons work in the diagnostic page vs actual app?" -ForegroundColor White
Write-Host "- Any JavaScript errors in browser console?" -ForegroundColor White
Write-Host "- Any failed network requests when clicking buttons?" -ForegroundColor White
Write-Host ""

# Keep the PowerShell window open
Write-Host "Press any key to close this diagnostic summary..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
