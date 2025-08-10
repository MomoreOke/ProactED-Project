# Restart ProactED Application
# This script safely stops and restarts the application with the latest changes

Write-Host "🔄 Restarting ProactED Application" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Stop the current application
try {
    Write-Host "🛑 Stopping current application (PID: 8100)..." -ForegroundColor Yellow
    Stop-Process -Id 8100 -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Write-Host "✅ Application stopped" -ForegroundColor Green
} catch {
    Write-Host "ℹ️ Application may have already stopped" -ForegroundColor Gray
}

# Build the application
Write-Host "🔨 Building application with latest changes..." -ForegroundColor Yellow
$buildResult = dotnet build --verbosity minimal
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    Write-Host "Build output:" -ForegroundColor Gray
    Write-Host $buildResult -ForegroundColor Gray
    exit 1
}

# Start the application
Write-Host "🚀 Starting application..." -ForegroundColor Yellow
Write-Host "The application will start with the MLApiStartupService which will:" -ForegroundColor Gray
Write-Host "• Automatically start the ML API if needed" -ForegroundColor Gray
Write-Host "• Ensure predictions are available immediately" -ForegroundColor Gray
Write-Host "• Display the new Days to Failure feature" -ForegroundColor Gray
Write-Host ""
Write-Host "📊 Dashboard will be available at:" -ForegroundColor Green
Write-Host "• https://localhost:5001/MLDashboard" -ForegroundColor White
Write-Host "• Features: Risk levels, Failure probability, Days to failure, Maintenance urgency" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop the application when testing is complete." -ForegroundColor Yellow

# Start the application (this will block)
dotnet run
