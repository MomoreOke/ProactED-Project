# 🧪 API Testing Comparison Guide

## How to Test with Both Tools

### 🔧 Setup Steps
1. **Start the ML API**: Run `start_ml_api.bat` or `python simple_ml_api.py`
2. **Verify API is running**: You should see "Server starting on http://localhost:5000"

---

## 📮 Postman Testing

### Import Collection
1. Open Postman
2. Click **Import**
3. Select `ProactED_ML_API_Tests.postman_collection.json`
4. Run each request in order

### Expected Postman Results

#### 1. Health Check
- **Status**: 200 OK
- **Response**:
```json
{
  "status": "healthy",
  "timestamp": "2025-08-07T...",
  "version": "1.0.0",
  "model_loaded": true,
  "model_type": "Simple Test Predictor",
  "message": "API is running and ready for predictions"
}
```

#### 2. Model Info
- **Status**: 200 OK
- **Response**:
```json
{
  "success": true,
  "model_version": "Simple-Test-Model-v1.0",
  "accuracy": 0.887,
  "features": ["age_months", "operating_temperature", "vibration_level", "power_consumption"]
}
```

#### 3. Single Prediction (Normal)
- **Status**: 200 OK
- **Response**:
```json
{
  "success": true,
  "equipment_id": "TEST_EQUIP_001",
  "failure_probability": 0.374,
  "risk_level": "Medium",
  "confidence_score": 0.803,
  "model_version": "Simple-Test-Model-v1.0",
  "prediction_timestamp": "..."
}
```

#### 4. High Risk Equipment
- **Status**: 200 OK
- **Expected**: `risk_level` should be "High" or "Critical"
- **Expected**: `failure_probability` should be > 0.5

#### 5. Low Risk Equipment
- **Status**: 200 OK
- **Expected**: `risk_level` should be "Low"
- **Expected**: `failure_probability` should be < 0.3

#### 6. Batch Prediction
- **Status**: 200 OK
- **Response**:
```json
{
  "success": true,
  "processed_count": 3,
  "predictions": [
    { "equipment_id": "BATCH_TEST_001", "risk_level": "...", ... },
    { "equipment_id": "BATCH_TEST_002", "risk_level": "...", ... },
    { "equipment_id": "BATCH_TEST_003", "risk_level": "...", ... }
  ]
}
```

#### 7. Error Test (Missing Fields)
- **Status**: 400 Bad Request
- **Response**:
```json
{
  "success": false,
  "error": "Missing required fields: ['operating_temperature', 'vibration_level', 'power_consumption']"
}
```

---

## 💻 curl Testing

### Run curl Tests
```batch
# Run the batch file
.\test_api_with_curl.bat
```

### Expected curl Results
- **All successful requests**: Return JSON responses with proper formatting
- **Error requests**: Return 400/404 status codes with error messages
- **Response times**: Should be < 1 second for all requests

---

## 🔍 Comparison Checklist

### ✅ Both Should Match:
- [ ] Health check returns "healthy" status
- [ ] Model info shows accuracy of 0.887
- [ ] Single predictions return realistic probabilities
- [ ] High risk equipment gets higher failure probability
- [ ] Low risk equipment gets lower failure probability
- [ ] Batch predictions process all equipment items
- [ ] Error handling works for missing fields
- [ ] Response JSON structure is consistent

### 🚨 Red Flags:
- ❌ Different failure probabilities for same input
- ❌ Inconsistent risk levels
- ❌ Different response formats
- ❌ Connection errors or timeouts
- ❌ Missing required fields in responses

---

## 🎯 Integration Readiness Criteria

### ✅ Ready for .NET Integration if:
1. **All health checks pass**
2. **Predictions return consistent, realistic values**
3. **Error handling works properly**
4. **Response format matches .NET DTO expectations**
5. **No connection issues or timeouts**

### ⚠️ Need to Fix if:
1. **Inconsistent responses between tools**
2. **Connection failures**
3. **Malformed JSON responses**
4. **Missing required fields**

---

## 🔧 Next Steps After Validation

Once both Postman and curl tests pass:

1. **Start your .NET application**
2. **Navigate to**: `https://localhost:7xxx/PredictionTest`
3. **Test the integration buttons**
4. **Monitor .NET logs** for API call details
5. **Compare results** with Postman/curl responses

---

## 📞 Troubleshooting

### If Tests Fail:
1. **Check API is running**: Visit http://localhost:5000 in browser
2. **Check port conflicts**: Ensure port 5000 isn't blocked
3. **Verify Python/Flask**: Restart `simple_ml_api.py`
4. **Check network**: Try `ping localhost`

### If Results Differ:
1. **Same input data**: Ensure identical JSON payloads
2. **Timing issues**: ML API uses some randomness - small differences are normal
3. **Connection issues**: Restart API and retry

---

🎉 **Once both tools show consistent, successful results, your .NET integration should work perfectly!**
