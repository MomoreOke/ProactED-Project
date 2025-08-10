# Enhanced Email Testing - Quick Start Guide
# 
# This script helps you quickly test the enhanced email functionality
# All emails will be sent to: noahjamal303@gmail.com

Write-Host "🚀 ProactED Enhanced Email Testing Guide" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green
Write-Host ""

Write-Host "📧 Email Configuration:" -ForegroundColor Cyan
Write-Host "  - Target Email: noahjamal303@gmail.com" -ForegroundColor White
Write-Host "  - SMTP: Gmail (smtp.gmail.com:587)" -ForegroundColor White  
Write-Host "  - Enhanced Templates: ✅ Active" -ForegroundColor Green
Write-Host "  - Equipment Specifications: ✅ Enabled" -ForegroundColor Green
Write-Host ""

Write-Host "🎯 Quick Testing Steps:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. BASIC TEST (5 minutes):" -ForegroundColor White
Write-Host "   → Go to: http://localhost:5261/Schedule/Create" -ForegroundColor Gray
Write-Host "   → Select Equipment: Any HVAC or Lighting equipment" -ForegroundColor Gray
Write-Host "   → Priority: MEDIUM" -ForegroundColor Gray
Write-Host "   → Description: 'Basic email functionality test'" -ForegroundColor Gray
Write-Host "   → Submit → Check noahjamal303@gmail.com" -ForegroundColor Gray
Write-Host ""

Write-Host "2. PRIORITY TEST (5 minutes):" -ForegroundColor White
Write-Host "   → Create another task with Priority: CRITICAL" -ForegroundColor Gray
Write-Host "   → Check for 🚨 CRITICAL subject line" -ForegroundColor Gray
Write-Host "   → Verify red/urgent formatting" -ForegroundColor Gray
Write-Host ""

Write-Host "3. EQUIPMENT SPECS TEST (5 minutes):" -ForegroundColor White  
Write-Host "   → Create task with different equipment type" -ForegroundColor Gray
Write-Host "   → Verify email contains:" -ForegroundColor Gray
Write-Host "     ✅ Equipment age calculation" -ForegroundColor Green
Write-Host "     ✅ Usage hours information" -ForegroundColor Green
Write-Host "     ✅ Warranty status" -ForegroundColor Green
Write-Host "     ✅ Location details (building/room)" -ForegroundColor Green
Write-Host "     ✅ Pre-maintenance checklist" -ForegroundColor Green
Write-Host ""

Write-Host "📊 Expected Email Features:" -ForegroundColor Magenta
Write-Host "  🎨 Priority-based subject lines:" -ForegroundColor White
Write-Host "     - 🚨 CRITICAL Priority" -ForegroundColor Red
Write-Host "     - ⚠️ HIGH Priority" -ForegroundColor Yellow
Write-Host "     - 📋 MEDIUM Priority" -ForegroundColor Blue
Write-Host "     - 📝 LOW Priority" -ForegroundColor Gray
Write-Host ""
Write-Host "  📋 Equipment Information Grid:" -ForegroundColor White
Write-Host "     - Model & Type Details" -ForegroundColor Gray
Write-Host "     - Installation Date & Age" -ForegroundColor Gray
Write-Host "     - Usage Statistics" -ForegroundColor Gray
Write-Host "     - Warranty Status" -ForegroundColor Gray
Write-Host "     - Location Information" -ForegroundColor Gray
Write-Host ""
Write-Host "  🛠️ Pre-Maintenance Checklist:" -ForegroundColor White
Write-Host "     - Safety Instructions" -ForegroundColor Gray
Write-Host "     - Required Tools" -ForegroundColor Gray
Write-Host "     - Shutdown Procedures" -ForegroundColor Gray
Write-Host ""

Write-Host "🌐 Application Status:" -ForegroundColor Cyan
$response = $null
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5261" -TimeoutSec 5 -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "  ✅ Application running on http://localhost:5261" -ForegroundColor Green
    }
} catch {
    Write-Host "  ❌ Application not accessible on http://localhost:5261" -ForegroundColor Red
    Write-Host "  → Run: dotnet run --project FEENALOoFINALE.csproj" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🔗 Quick Links:" -ForegroundColor Cyan
Write-Host "  📋 Create Task: http://localhost:5261/Schedule/Create" -ForegroundColor Blue
Write-Host "  📊 Dashboard: http://localhost:5261" -ForegroundColor Blue
Write-Host "  📧 Check Email: https://gmail.com (noahjamal303@gmail.com)" -ForegroundColor Blue
Write-Host ""

Write-Host "⚡ FASTEST TEST:" -ForegroundColor Yellow
Write-Host "1. Click: http://localhost:5261/Schedule/Create" -ForegroundColor White
Write-Host "2. Fill form → Submit" -ForegroundColor White
Write-Host "3. Check: noahjamal303@gmail.com" -ForegroundColor White
Write-Host "4. Look for: Enhanced equipment specifications!" -ForegroundColor Green
Write-Host ""

Write-Host "📝 Success Indicators:" -ForegroundColor Green
Write-Host "  ✅ Email received within 2-3 minutes" -ForegroundColor White
Write-Host "  ✅ Subject has priority emoji (🚨⚠️📋📝)" -ForegroundColor White
Write-Host "  ✅ Professional HTML formatting" -ForegroundColor White
Write-Host "  ✅ Equipment age calculation shown" -ForegroundColor White
Write-Host "  ✅ Usage hours and warranty info" -ForegroundColor White
Write-Host "  ✅ Building and room location" -ForegroundColor White
Write-Host "  ✅ Pre-maintenance checklist included" -ForegroundColor White
Write-Host ""

Write-Host "🚨 Troubleshooting:" -ForegroundColor Red
Write-Host "  No Email? → Check Gmail spam/junk folders" -ForegroundColor White
Write-Host "  Incomplete Data? → Try different equipment from dropdown" -ForegroundColor White
Write-Host "  App Issues? → Check terminal logs for errors" -ForegroundColor White
Write-Host ""

Write-Host "🎯 Ready to test! Your enhanced email system awaits! 🚀" -ForegroundColor Green
Write-Host ""

# Open browser if application is running
if ($response -and $response.StatusCode -eq 200) {
    Write-Host "🌐 Opening browser for quick testing..." -ForegroundColor Cyan
    Start-Process "http://localhost:5261/Schedule/Create"
}
