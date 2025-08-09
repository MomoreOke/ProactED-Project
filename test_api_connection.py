"""
Simple test script to verify ML API is working
Run this to test the API endpoints before using the .NET integration
"""

import requests
import json
import sys

def test_api():
    base_url = "http://localhost:5000"
    
    print("🧪 Testing ProactED ML API Integration...")
    print("=" * 50)
    
    # Test 1: Health Check
    print("\n1. Testing Health Check...")
    try:
        response = requests.get(f"{base_url}/api/health", timeout=5)
        if response.status_code == 200:
            print("✅ Health check passed")
            print(f"   Response: {response.json()}")
        else:
            print(f"❌ Health check failed: {response.status_code}")
            return False
    except requests.exceptions.ConnectionError:
        print("❌ Cannot connect to API. Is it running on localhost:5000?")
        return False
    except Exception as e:
        print(f"❌ Health check error: {e}")
        return False
    
    # Test 2: Model Info
    print("\n2. Testing Model Info...")
    try:
        response = requests.get(f"{base_url}/api/model/info", timeout=5)
        if response.status_code == 200:
            print("✅ Model info retrieved")
            model_info = response.json()
            print(f"   Model Version: {model_info.get('model_version', 'Unknown')}")
            print(f"   Accuracy: {model_info.get('accuracy', 'Unknown')}")
        else:
            print(f"❌ Model info failed: {response.status_code}")
    except Exception as e:
        print(f"❌ Model info error: {e}")
    
    # Test 3: Single Prediction
    print("\n3. Testing Single Prediction...")
    try:
        test_data = {
            "equipment_id": "TEST_EQUIP_001",
            "age_months": 24,
            "operating_temperature": 75.5,
            "vibration_level": 3.2,
            "power_consumption": 1500.0
        }
        
        response = requests.post(
            f"{base_url}/api/equipment/predict", 
            json=test_data,
            headers={"Content-Type": "application/json"},
            timeout=10
        )
        
        if response.status_code == 200:
            print("✅ Single prediction successful")
            result = response.json()
            print(f"   Equipment ID: {result.get('equipment_id')}")
            print(f"   Risk Level: {result.get('risk_level')}")
            print(f"   Failure Probability: {result.get('failure_probability', 0):.1%}")
            print(f"   Confidence: {result.get('confidence_score', 0):.1%}")
        else:
            print(f"❌ Single prediction failed: {response.status_code}")
            print(f"   Response: {response.text}")
    except Exception as e:
        print(f"❌ Single prediction error: {e}")
    
    # Test 4: Batch Prediction
    print("\n4. Testing Batch Prediction...")
    try:
        batch_data = {
            "equipment_list": [
                {
                    "equipment_id": "BATCH_TEST_001",
                    "age_months": 12,
                    "operating_temperature": 65.0,
                    "vibration_level": 2.1,
                    "power_consumption": 1200.0
                },
                {
                    "equipment_id": "BATCH_TEST_002",
                    "age_months": 36,
                    "operating_temperature": 85.0,
                    "vibration_level": 4.5,
                    "power_consumption": 1800.0
                }
            ]
        }
        
        response = requests.post(
            f"{base_url}/api/equipment/batch-predict",
            json=batch_data,
            headers={"Content-Type": "application/json"},
            timeout=15
        )
        
        if response.status_code == 200:
            print("✅ Batch prediction successful")
            result = response.json()
            print(f"   Processed Count: {result.get('processed_count', 0)}")
            
            predictions = result.get('predictions', [])
            for i, pred in enumerate(predictions[:2]):  # Show first 2
                print(f"   Equipment {i+1}: {pred.get('equipment_id')} - {pred.get('risk_level')}")
        else:
            print(f"❌ Batch prediction failed: {response.status_code}")
            print(f"   Response: {response.text}")
    except Exception as e:
        print(f"❌ Batch prediction error: {e}")
    
    print("\n" + "=" * 50)
    print("🎉 API Testing Complete!")
    print("\nIf all tests passed, your .NET integration should work.")
    print("If any tests failed, check the ML API logs for errors.")
    
    return True

if __name__ == "__main__":
    test_api()
