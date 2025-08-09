"""
Test the REAL trained model API endpoints
"""

import requests
import json

def test_real_model_api():
    base_url = "http://localhost:5000"
    
    print("🧠 Testing REAL ProactED ML API with Trained Model...")
    print("=" * 60)
    
    # Test 1: Health Check
    print("\n1. Testing Health Check...")
    try:
        response = requests.get(f"{base_url}/api/health", timeout=5)
        if response.status_code == 200:
            health_data = response.json()
            print("✅ Health check passed")
            print(f"   Status: {health_data.get('status')}")
            print(f"   Model Loaded: {health_data.get('model_loaded')}")
            print(f"   Model Type: {health_data.get('model_type')}")
        else:
            print(f"❌ Health check failed: {response.status_code}")
    except Exception as e:
        print(f"❌ Health check error: {e}")
    
    # Test 2: Model Info (Real Model)
    print("\n2. Testing REAL Model Information...")
    try:
        response = requests.get(f"{base_url}/api/model/info", timeout=5)
        if response.status_code == 200:
            model_info = response.json()
            print("✅ Real model info retrieved")
            print(f"   Model Version: {model_info.get('model_version')}")
            print(f"   R² Score: {model_info.get('r2_score', model_info.get('accuracy'))}")
            print(f"   Feature Count: {model_info.get('feature_count')}")
            print(f"   Features: {model_info.get('features')}")
            print(f"   Model Type: {model_info.get('model_type')}")
            if 'note' in model_info:
                print(f"   Note: {model_info.get('note')}")
        else:
            print(f"❌ Model info failed: {response.status_code}")
    except Exception as e:
        print(f"❌ Model info error: {e}")
    
    # Test 3: Single Prediction with Real Model
    print("\n3. Testing Single Prediction (Real Model)...")
    try:
        test_data = {
            "equipment_id": "REAL_MODEL_TEST_001",
            "age_months": 36,
            "operating_temperature": 85.0,
            "vibration_level": 4.2,
            "power_consumption": 1800.0
        }
        
        response = requests.post(
            f"{base_url}/api/equipment/predict",
            json=test_data,
            headers={"Content-Type": "application/json"},
            timeout=10
        )
        
        if response.status_code == 200:
            result = response.json()
            print("✅ Real model prediction successful")
            print(f"   Equipment ID: {result.get('equipment_id')}")
            print(f"   Risk Level: {result.get('risk_level')}")
            print(f"   Failure Probability: {result.get('failure_probability', 0):.1%}")
            print(f"   Confidence Score: {result.get('confidence_score', 0):.1%}")
            print(f"   Model Version: {result.get('model_version')}")
            print(f"   R² Score: {result.get('r2_score')}")
            print(f"   Features Used: {result.get('model_features_used')}")
            if 'note' in result:
                print(f"   Note: {result.get('note')}")
            
            # Show feature importance if available
            if 'feature_importance' in result and result['feature_importance']:
                print(f"   Top Feature Importances:")
                importance = result['feature_importance']
                sorted_features = sorted(importance.items(), key=lambda x: x[1], reverse=True)
                for feature, score in sorted_features[:4]:  # Show top 4
                    print(f"     - {feature}: {score:.3f}")
                    
        else:
            print(f"❌ Real model prediction failed: {response.status_code}")
            print(f"   Response: {response.text}")
    except Exception as e:
        print(f"❌ Real model prediction error: {e}")
    
    # Test 4: High Risk Equipment (Should trigger higher probability)
    print("\n4. Testing High Risk Equipment...")
    try:
        high_risk_data = {
            "equipment_id": "HIGH_RISK_EQUIPMENT",
            "age_months": 60,  # Very old
            "operating_temperature": 95.0,  # Very hot
            "vibration_level": 8.0,  # High vibration
            "power_consumption": 2500.0  # High power
        }
        
        response = requests.post(
            f"{base_url}/api/equipment/predict",
            json=high_risk_data,
            headers={"Content-Type": "application/json"},
            timeout=10
        )
        
        if response.status_code == 200:
            result = response.json()
            print("✅ High risk prediction successful")
            print(f"   Risk Level: {result.get('risk_level')}")
            print(f"   Failure Probability: {result.get('failure_probability', 0):.1%}")
            print(f"   Expected: Should be High/Critical risk with >50% probability")
        else:
            print(f"❌ High risk prediction failed: {response.status_code}")
    except Exception as e:
        print(f"❌ High risk prediction error: {e}")
    
    print("\n" + "=" * 60)
    print("🎉 Real Model API Testing Complete!")
    print("\n🔍 Key Indicators of Real Model:")
    print("   ✓ Model version contains 'production-v2.0'")
    print("   ✓ R² Score around 0.91 (91%)")
    print("   ✓ 8 features instead of 4")
    print("   ✓ Feature importance data included")
    print("   ✓ More realistic failure probabilities")
    print("\n🚀 If all tests show these indicators, integration is ready!")

if __name__ == "__main__":
    test_real_model_api()
