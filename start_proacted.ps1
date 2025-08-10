# ProactED Complete Startup Script
Write-Host "🚀 ProactED Complete Startup Script" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green

Write-Host "`n🤖 Step 1: Ensuring ML API is running..." -ForegroundColor Cyan
& ".\ensure_ml_api.ps1"

Write-Host "`n🎯 Step 2: Starting ProactED Application..." -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow
Write-Host ""

dotnet run
