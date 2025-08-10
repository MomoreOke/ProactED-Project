# PowerShell script to ensure ML API is running before starting ProactED
Write-Host "ProactED ML API Checker & Starter" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# Configuration
$mlApiUrl = "http://localhost:5001"
$healthEndpoint = "$mlApiUrl/api/health"
$mlApiDirectory = Join-Path $PSScriptRoot "ml_api"
$appPyPath = Join-Path $mlApiDirectory "app.py"
$maxWaitTime = 30 # seconds

function Test-MLApiHealth {
    try {
        Write-Host "Checking ML API health at: $healthEndpoint" -ForegroundColor Yellow
        $response = Invoke-RestMethod -Uri $healthEndpoint -TimeoutSec 5 -ErrorAction Stop
        
        if ($response.status -eq "healthy") {
            Write-Host "ML API is healthy and running!" -ForegroundColor Green
            return $true
        } else {
            Write-Host "ML API responded but not healthy" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host "ML API is not responding" -ForegroundColor Red
        return $false
    }
}

function Start-MLApi {
    Write-Host "Starting ML API..." -ForegroundColor Cyan
    
    # Check if ml_api directory exists
    if (-not (Test-Path $mlApiDirectory)) {
        Write-Host "ML API directory not found at: $mlApiDirectory" -ForegroundColor Red
        return $false
    }
    
    # Check if app.py exists
    if (-not (Test-Path $appPyPath)) {
        Write-Host "app.py not found at: $appPyPath" -ForegroundColor Red
        return $false
    }
    
    # Check if Python is available
    try {
        $pythonVersion = python --version 2>$null
        Write-Host "Python found: $pythonVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "Python not found. Please install Python and ensure it's in PATH." -ForegroundColor Red
        return $false
    }
    
    # Start the ML API in a new PowerShell window
    try {
        Write-Host "Launching ML API in new window..." -ForegroundColor Yellow
        
        $command = "cd '$mlApiDirectory'; Write-Host 'Starting ProactED ML API...' -ForegroundColor Green; python app.py"
        
        Start-Process -FilePath "powershell" -ArgumentList "-NoExit", "-Command", $command -WindowStyle Normal
        
        Write-Host "Waiting for ML API to start..." -ForegroundColor Yellow
        
        # Wait for API to become healthy
        $waitCount = 0
        while ($waitCount -lt $maxWaitTime) {
            Start-Sleep -Seconds 1
            $waitCount++
            
            if (Test-MLApiHealth) {
                Write-Host "ML API started successfully in $waitCount seconds!" -ForegroundColor Green
                return $true
            }
            
            if ($waitCount % 5 -eq 0) {
                Write-Host "Still waiting... ($waitCount/$maxWaitTime seconds)" -ForegroundColor Yellow
            }
        }
        
        Write-Host "ML API failed to start within $maxWaitTime seconds" -ForegroundColor Red
        return $false
    }
    catch {
        Write-Host "Error starting ML API: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Main logic
Write-Host ""
Write-Host "Step 1: Checking if ML API is already running..." -ForegroundColor Cyan

if (Test-MLApiHealth) {
    Write-Host "ML API is already running and healthy!" -ForegroundColor Green
    Write-Host "You can now start ProactED with: dotnet run" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "Step 2: Starting ML API..." -ForegroundColor Cyan
    
    if (Start-MLApi) {
        Write-Host ""
        Write-Host "ML API is now running!" -ForegroundColor Green
        Write-Host "You can now start ProactED with: dotnet run" -ForegroundColor Green
        Write-Host "ML API Dashboard: $mlApiUrl" -ForegroundColor Cyan
        Write-Host "Health Check: $healthEndpoint" -ForegroundColor Cyan
    } else {
        Write-Host ""
        Write-Host "Failed to start ML API!" -ForegroundColor Red
        Write-Host "ProactED will run in fallback mode (mock predictions)" -ForegroundColor Yellow
        Write-Host "To fix this issue:" -ForegroundColor Yellow
        Write-Host "   1. Ensure Python is installed and in PATH" -ForegroundColor White
        Write-Host "   2. Check that ml_api/app.py exists" -ForegroundColor White
        Write-Host "   3. Install Python dependencies: pip install -r ml_api/requirements.txt" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "================================" -ForegroundColor Green
Write-Host "ProactED ML API Check Complete" -ForegroundColor Green
