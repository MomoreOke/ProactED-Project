# PowerShell script to start the ProactED ML API
Write-Host "🚀 Starting ProactED ML API..." -ForegroundColor Green

# Check if Python is available
try {
    $pythonVersion = python --version 2>$null
    Write-Host "✅ Python found: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Python not found. Please install Python first." -ForegroundColor Red
    exit 1
}

# Navigate to ml_api directory
Set-Location "c:\Users\NABILA\Desktop\ProactED-Project\ml_api"

# Check if virtual environment exists
if (Test-Path "venv") {
    Write-Host "📦 Activating virtual environment..." -ForegroundColor Yellow
    .\venv\Scripts\Activate.ps1
} else {
    Write-Host "⚠️  No virtual environment found. Installing packages globally..." -ForegroundColor Yellow
    pip install -r requirements.txt
}

Write-Host "🌐 Starting Flask API on http://localhost:5001..." -ForegroundColor Cyan
Write-Host "🔗 Ready for .NET integration testing!" -ForegroundColor Green
Write-Host "" 
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

# Start the Flask application
python app.py
