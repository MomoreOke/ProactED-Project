# 🚀 Quick Start Guide - ProactED ML Integration Testing

## Step 1: Start the ML API

**Option A: Using the PowerShell script (Recommended)**
```powershell
# Navigate to your project folder
cd "c:\Users\NABILA\Desktop\ProactED-Project"

# Run the startup script
.\start_ml_api.ps1
```

**Option B: Manual startup**
```powershell
# Navigate to the ML API directory
cd "c:\Users\NABILA\Desktop\ProactED-Project\ml_api"

# Install required packages (if not already installed)
pip install -r requirements.txt

# Start the Flask API
python app.py
```

## Step 2: Verify API is Running

The API should start on `http://localhost:5000` and you should see:
```
🤖 Starting ProactED Production ML API with Trained Random Forest Model
📡 API Endpoints:
   GET  /api/health                    - Health check
   GET  /api/model/info                - Model information
   POST /api/equipment/predict         - Single equipment prediction
   POST /api/equipment/batch-predict   - Batch equipment predictions
🚀 Server starting on http://localhost:5000
🔗 Ready for .NET ProactED integration!
```

## Step 3: Test the Integration

1. **Keep the ML API running** in one terminal/PowerShell window
2. **Start your .NET application** in Visual Studio or with `dotnet run`
3. **Navigate to the test page**: `https://localhost:7xxx/PredictionTest`
4. **Click the test buttons** to verify integration

## API Endpoints Available

- `GET /` - API documentation
- `GET /api/health` - Health check (should return "healthy")  
- `GET /api/model/info` - Model information
- `POST /api/equipment/predict` - Single prediction
- `POST /api/equipment/batch-predict` - Batch predictions

## Expected Test Results

✅ **Single Prediction Test**: Should return failure probability, risk level, and confidence score  
✅ **Batch Prediction Test**: Should process multiple equipment items and return predictions array  
✅ **Real-time Updates**: Results should appear in the web interface  

## Troubleshooting

**If the API fails to start:**
- Ensure Python is installed and accessible
- Install missing packages: `pip install flask flask-cors pandas scikit-learn numpy`
- Check if port 5000 is available

**If integration tests fail:**
- Verify the ML API is running on http://localhost:5000
- Check browser developer console for error messages
- Ensure .NET app is configured with correct API URL

## Next Steps

Once integration testing is successful, you can:
1. Deploy the ML API to a production server
2. Update the .NET configuration to use the production URL
3. Implement additional ML features like model retraining
4. Add monitoring and logging for production use

🎉 **You're ready to test the ProactED ML integration!**
