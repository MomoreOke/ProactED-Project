@echo off
echo 🤖 Starting ProactED REAL ML API with Trained Model...
echo.

REM Check if Python is available
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Python not found. Please install Python from https://python.org
    echo.
    pause
    exit /b 1
)

REM Navigate to ML API directory (where the real app.py and model are)
cd /d "c:\Users\NABILA\Desktop\ProactED-Project\ml_api"

echo ✅ Python found
echo 📦 Installing required packages...
pip install flask flask-cors pandas scikit-learn numpy >nul 2>&1

echo 🧠 Starting REAL ML API with trained Random Forest model...
echo 🌐 Server starting on http://localhost:5000...
echo 🔗 Using actual trained model with 91%% R² accuracy!
echo 💡 This version uses the real pickle file with 8 features
echo.
echo Press Ctrl+C to stop the server
echo.

REM Start the real Flask API
python app.py
