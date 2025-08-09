# ML Prediction API for ProactED Integration

Simple Flask API to simulate machine learning predictions for equipment failure analysis.

## Setup Instructions

1. **Install Python Dependencies**:
```bash
pip install flask numpy
```

2. **Run the API**:
```bash
cd ml_api
python app.py
```

3. **API will be available at**: `http://localhost:5000`

## API Endpoints

### Health Check
- **GET** `/health` - Check if API is running

### Model Information
- **GET** `/model/info` - Get model details and accuracy

### Predictions
- **POST** `/predict` - Single equipment prediction
- **POST** `/predict/batch` - Batch equipment predictions

### Example Request (Single Prediction)
```json
{
    "equipment_id": "123",
    "age_months": 24,
    "operating_temperature": 65.5,
    "vibration_level": 2.3,
    "power_consumption": 250.0
}
```

### Example Response
```json
{
    "success": true,
    "equipment_id": "123",
    "failure_probability": 0.342,
    "risk_level": "Medium",
    "confidence_score": 0.876,
    "prediction_timestamp": "2024-12-24T10:30:00",
    "model_version": "test-v1.0",
    "feature_importance": {
        "age_months": 0.200,
        "operating_temperature": 0.355,
        "vibration_level": 0.230,
        "power_consumption": 0.125
    }
}
```

## Integration with ProactED

1. **Update appsettings.json**:
```json
{
    "MLPredictionApi": {
        "BaseUrl": "http://localhost:5000",
        "TimeoutSeconds": 30
    }
}
```

2. **Start both applications**:
   - Run the Python API: `python ml_api/app.py`
   - Run ProactED: `dotnet run`

3. **Navigate to**: `http://localhost:5000/MLPredictiveMaintenance`

## Notes

- This is a **simulation API** for testing integration
- Real ML models would require training data and proper algorithms
- The prediction logic uses simple weighted calculations for demonstration
- In production, replace with actual trained ML models (scikit-learn, TensorFlow, etc.)
