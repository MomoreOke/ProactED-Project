@echo off
echo 🚀 ProactED Complete Startup Script
echo ====================================

echo.
echo 🤖 Step 1: Ensuring ML API is running...
powershell -ExecutionPolicy Bypass -File "ensure_ml_api.ps1"

echo.
echo 🎯 Step 2: Starting ProactED Application...
echo Press Ctrl+C to stop the application
echo.

dotnet run
