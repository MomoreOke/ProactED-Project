# 🚀 Quick Start Test Script for ML API Integration
# Run this script to test if the integration is working

import requests
import json
import time
import sys

# Configuration
API_BASE_URL = "http://localhost:5000"
TEST_ENDPOINTS = {
    "root": f"{API_BASE_URL}/",
    "health": f"{API_BASE_URL}/api/health",
    "model_info": f"{API_BASE_URL}/api/model/info",
    "single_predict": f"{API_BASE_URL}/api/equipment/predict",
    "batch_predict": f"{API_BASE_URL}/api/equipment/batch-predict"
}

def test_api_connection():
    """Test basic API connectivity"""
    print("🔗 Testing API Connection...")
    try:
        response = requests.get(TEST_ENDPOINTS["root"], timeout=5)
        if response.status_code == 200:
            data = response.json()
            print("✅ API is responding!")
            print(f"   Message: {data.get('message', 'Unknown')}")
            print(f"   Version: {data.get('version', 'Unknown')}")
            return True
        else:
            print(f"❌ API returned status code: {response.status_code}")
            return False
    except requests.exceptions.ConnectionError:
        print("❌ Connection failed - API is not running")
        print("💡 Make sure to start the API first:")
        print("   1. Open terminal/PowerShell")
        print("   2. Navigate to: c:\\Users\\NABILA\\Desktop\\Predictive Model")
        print("   3. Run: python enhanced_equipment_api.py")
        return False
    except Exception as e:
        print(f"❌ Error: {e}")
        return False

def test_health_endpoint():
    """Test health endpoint"""
    print("\n🩺 Testing Health Endpoint...")
    try:
        response = requests.get(TEST_ENDPOINTS["health"], timeout=5)
        if response.status_code == 200:
            data = response.json()
            print("✅ Health endpoint working!")
            print(f"   Status: {data.get('status', 'Unknown')}")
            print(f"   Model Loaded: {data.get('model_loaded', 'Unknown')}")
            return True
        else:
            print(f"❌ Health endpoint failed with status: {response.status_code}")
            return False
    except Exception as e:
        print(f"❌ Health endpoint error: {e}")
        return False

def test_model_info():
    """Test model info endpoint"""
    print("\n📊 Testing Model Info Endpoint...")
    try:
        response = requests.get(TEST_ENDPOINTS["model_info"], timeout=5)
        if response.status_code == 200:
            data = response.json()
            print("✅ Model info endpoint working!")
            print(f"   Model Version: {data.get('model_version', 'Unknown')}")
            print(f"   Accuracy: {data.get('accuracy', 'Unknown')}")
            print(f"   Features: {len(data.get('features', []))}")
            return True
        else:
            print(f"❌ Model info failed with status: {response.status_code}")
            return False
    except Exception as e:
        print(f"❌ Model info error: {e}")
        return False

def test_single_prediction():
    """Test single prediction endpoint"""
    print("\n🤖 Testing Single Prediction...")
    
    test_data = {
        "equipment_id": "TEST001",
        "age_months": 24,
        "operating_temperature": 75.5,
        "vibration_level": 2.3,
        "power_consumption": 150.0
    }
    
    try:
        response = requests.post(
            TEST_ENDPOINTS["single_predict"], 
            json=test_data, 
            headers={"Content-Type": "application/json"},
            timeout=10
        )
        
        if response.status_code == 200:
            data = response.json()
            print("✅ Single prediction working!")
            print(f"   Equipment ID: {data.get('equipment_id', 'Unknown')}")
            print(f"   Risk Level: {data.get('risk_level', 'Unknown')}")
            print(f"   Failure Probability: {data.get('failure_probability', 0):.1%}")
            print(f"   Confidence: {data.get('confidence_score', 0):.1%}")
            return True
        else:
            print(f"❌ Single prediction failed with status: {response.status_code}")
            print(f"   Response: {response.text}")
            return False
    except Exception as e:
        print(f"❌ Single prediction error: {e}")
        return False

def test_batch_prediction():
    """Test batch prediction endpoint"""
    print("\n📦 Testing Batch Prediction...")
    
    test_data = {
        "equipment_list": [
            {
                "equipment_id": "TEST001",
                "age_months": 24,
                "operating_temperature": 75.5,
                "vibration_level": 2.3,
                "power_consumption": 150.0
            },
            {
                "equipment_id": "TEST002",
                "age_months": 36,
                "operating_temperature": 85.0,
                "vibration_level": 4.2,
                "power_consumption": 180.0
            }
        ]
    }
    
    try:
        response = requests.post(
            TEST_ENDPOINTS["batch_predict"], 
            json=test_data, 
            headers={"Content-Type": "application/json"},
            timeout=15
        )
        
        if response.status_code == 200:
            data = response.json()
            print("✅ Batch prediction working!")
            print(f"   Processed Count: {data.get('processed_count', 0)}")
            print(f"   Predictions: {len(data.get('predictions', []))}")
            
            # Show first prediction details
            predictions = data.get('predictions', [])
            if predictions:
                first = predictions[0]
                print(f"   First Result - Equipment: {first.get('equipment_id')}, Risk: {first.get('risk_level')}")
                
            return True
        else:
            print(f"❌ Batch prediction failed with status: {response.status_code}")
            print(f"   Response: {response.text}")
            return False
    except Exception as e:
        print(f"❌ Batch prediction error: {e}")
        return False

def main():
    """Run all tests"""
    print("🔧 ProactED ML API Integration Test")
    print("=" * 50)
    
    tests = [
        ("API Connection", test_api_connection),
        ("Health Endpoint", test_health_endpoint),
        ("Model Info", test_model_info),
        ("Single Prediction", test_single_prediction),
        ("Batch Prediction", test_batch_prediction)
    ]
    
    passed = 0
    total = len(tests)
    
    for test_name, test_func in tests:
        if test_func():
            passed += 1
        time.sleep(0.5)  # Small delay between tests
    
    print("\n" + "=" * 50)
    print(f"📊 Test Results: {passed}/{total} tests passed")
    
    if passed == total:
        print("🎉 All tests passed! Integration is working correctly.")
        print("\n✅ Next Steps:")
        print("   1. Navigate to your .NET application")
        print("   2. Go to: /PredictionTest")
        print("   3. Test the integration from the web interface")
    else:
        print("⚠️  Some tests failed. Please check the API configuration.")
        print("\n🔧 Troubleshooting:")
        print("   1. Make sure the Python ML API is running")
        print("   2. Check if port 5000 is available")
        print("   3. Verify the enhanced_equipment_api.py file exists")
    
    return passed == total

if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1)
