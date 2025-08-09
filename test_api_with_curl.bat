@echo off
echo 🧪 ProactED ML API - Comprehensive curl Testing
echo ================================================
echo.

set API_BASE=http://localhost:5000

echo 1. Testing Health Check...
echo ----------------------------------------
curl -X GET %API_BASE%/api/health
echo.
echo.

echo 2. Testing Model Information...
echo ----------------------------------------
curl -X GET %API_BASE%/api/model/info
echo.
echo.

echo 3. Testing Single Equipment Prediction (Normal Risk)...
echo ----------------------------------------
curl -X POST %API_BASE%/api/equipment/predict ^
  -H "Content-Type: application/json" ^
  -d "{\"equipment_id\":\"TEST_EQUIP_001\",\"age_months\":24,\"operating_temperature\":75.5,\"vibration_level\":3.2,\"power_consumption\":1500.0}"
echo.
echo.

echo 4. Testing High Risk Equipment...
echo ----------------------------------------
curl -X POST %API_BASE%/api/equipment/predict ^
  -H "Content-Type: application/json" ^
  -d "{\"equipment_id\":\"HIGH_RISK_EQUIP\",\"age_months\":48,\"operating_temperature\":95.0,\"vibration_level\":8.5,\"power_consumption\":2500.0}"
echo.
echo.

echo 5. Testing Low Risk Equipment...
echo ----------------------------------------
curl -X POST %API_BASE%/api/equipment/predict ^
  -H "Content-Type: application/json" ^
  -d "{\"equipment_id\":\"LOW_RISK_EQUIP\",\"age_months\":3,\"operating_temperature\":45.0,\"vibration_level\":1.2,\"power_consumption\":1000.0}"
echo.
echo.

echo 6. Testing Batch Prediction...
echo ----------------------------------------
curl -X POST %API_BASE%/api/equipment/batch-predict ^
  -H "Content-Type: application/json" ^
  -d "{\"equipment_list\":[{\"equipment_id\":\"BATCH_001\",\"age_months\":12,\"operating_temperature\":65.0,\"vibration_level\":2.1,\"power_consumption\":1200.0},{\"equipment_id\":\"BATCH_002\",\"age_months\":36,\"operating_temperature\":85.0,\"vibration_level\":4.5,\"power_consumption\":1800.0}]}"
echo.
echo.

echo 7. Testing Error Handling (Missing Fields)...
echo ----------------------------------------
curl -X POST %API_BASE%/api/equipment/predict ^
  -H "Content-Type: application/json" ^
  -d "{\"equipment_id\":\"INCOMPLETE_DATA\",\"age_months\":24}"
echo.
echo.

echo 8. Testing Invalid Endpoint...
echo ----------------------------------------
curl -X GET %API_BASE%/api/invalid/endpoint
echo.
echo.

echo ================================================
echo 🎉 curl Testing Complete!
echo.
echo Compare these results with your Postman tests.
echo All successful requests should return JSON responses.
echo.
pause
