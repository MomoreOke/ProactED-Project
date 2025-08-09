@echo off
echo 🚀 Starting ProactED Simplified ML API...
echo.

REM Check if Python is available
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Python not found. Please install Python from https://python.org
    echo.
    pause
    exit /b 1
)

REM Navigate to project root directory (where simple_ml_api.py is located)
cd /d "c:\Users\NABILA\Desktop\ProactED-Project"

echo ✅ Python found
echo 📦 Installing basic required packages...
pip install flask >nul 2>&1

echo 🌐 Starting Simplified ML API on http://localhost:5000...
echo 🔗 Ready for .NET integration testing!
echo 💡 Using simplified API that doesn't require ML model files
echo.
echo Press Ctrl+C to stop the server
echo.

REM Start the simplified Flask API
python simple_ml_api.py
