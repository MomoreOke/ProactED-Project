"""
Test script to verify the trained model can be loaded
"""

import pickle
import os
import pandas as pd
import numpy as np

def test_model_loading():
    """Test loading the trained model pickle file"""
    print("🔍 Testing model loading...")
    
    # Try different paths
    possible_paths = [
        'complete_equipment_failure_prediction_system.pkl',
        '../complete_equipment_failure_prediction_system.pkl',
        os.path.join('..', 'Predictive Model', 'complete_equipment_failure_prediction_system.pkl'),
        os.path.join('ml_api', 'complete_equipment_failure_prediction_system.pkl')
    ]
    
    model_system = None
    loaded_from = None
    
    for path in possible_paths:
        try:
            if os.path.exists(path):
                print(f"✓ Found model file at: {path}")
                with open(path, 'rb') as f:
                    model_system = pickle.load(f)
                loaded_from = path
                print(f"✅ Successfully loaded model from: {path}")
                break
            else:
                print(f"✗ Model not found at: {path}")
        except Exception as e:
            print(f"❌ Failed to load from {path}: {e}")
    
    if model_system is None:
        print("❌ Could not load model from any path")
        return False
    
    # Analyze the model structure
    print(f"\n📊 Model Analysis:")
    print(f"Model type: {type(model_system)}")
    
    if isinstance(model_system, dict):
        print(f"Dictionary keys: {list(model_system.keys())}")
        
        # Check for common model components
        if 'model_info' in model_system:
            model_info = model_system['model_info']
            print(f"Model info keys: {list(model_info.keys()) if isinstance(model_info, dict) else type(model_info)}")
            
            if isinstance(model_info, dict):
                if 'model_object' in model_info:
                    model_obj = model_info['model_object']
                    print(f"Model object type: {type(model_obj)}")
                    if hasattr(model_obj, 'feature_importances_'):
                        print(f"Features count: {len(model_obj.feature_importances_)}")
                
                if 'features' in model_info:
                    features = model_info['features']
                    print(f"Feature names: {features}")
                
                if 'performance_metrics' in model_info:
                    metrics = model_info['performance_metrics']
                    print(f"Performance metrics: {metrics}")
                    
        if 'scaler' in model_system:
            scaler = model_system['scaler']
            print(f"Scaler type: {type(scaler)}")
    
    # Test a simple prediction
    print(f"\n🧪 Testing prediction...")
    try:
        # Create sample data
        sample_data = {
            'equipment_id': 'TEST_001',
            'age_months': 24,
            'operating_temperature': 75.5,
            'vibration_level': 3.2,
            'power_consumption': 1500.0
        }
        
        df = pd.DataFrame([sample_data])
        
        # Try to make a prediction
        if isinstance(model_system, dict) and 'model_info' in model_system:
            model_info = model_system['model_info']
            if isinstance(model_info, dict) and 'model_object' in model_info:
                model = model_info['model_object']
                features = model_info.get('features', ['age_months', 'operating_temperature', 'vibration_level', 'power_consumption'])
                
                # Ensure required features exist
                for feature in features:
                    if feature not in df.columns:
                        # Try to map common variations
                        if feature in ['Age_Months', 'age_months']:
                            df[feature] = df.get('age_months', 0)
                        elif feature in ['Operating_Temperature', 'operating_temperature']:
                            df[feature] = df.get('operating_temperature', 0)
                        elif feature in ['Vibration_Level', 'vibration_level']:
                            df[feature] = df.get('vibration_level', 0)
                        elif feature in ['Power_Consumption', 'power_consumption']:
                            df[feature] = df.get('power_consumption', 0)
                        else:
                            df[feature] = 0
                
                X = df[features].fillna(0)
                
                # Apply scaling if available
                if 'scaler' in model_system:
                    X = model_system['scaler'].transform(X)
                
                # Make prediction
                if hasattr(model, 'predict_proba'):
                    prediction = model.predict_proba(X)[0]
                    failure_prob = prediction[1] if len(prediction) > 1 else prediction[0]
                else:
                    failure_prob = model.predict(X)[0]
                
                print(f"✅ Test prediction successful!")
                print(f"   Failure probability: {failure_prob:.1%}")
                print(f"   Model features: {features}")
                
                return True, model_system, loaded_from
                
    except Exception as e:
        print(f"❌ Prediction test failed: {e}")
        return False, model_system, loaded_from
    
    return True, model_system, loaded_from

if __name__ == "__main__":
    # Change to ml_api directory
    try:
        os.chdir('ml_api')
        print("📁 Changed to ml_api directory")
    except:
        print("📁 Already in correct directory or ml_api not found")
    
    success, model, path = test_model_loading()
    
    if success and model:
        print(f"\n🎉 Model loading test successful!")
        print(f"📍 Model loaded from: {path}")
        print(f"🚀 Ready to integrate with Flask API!")
    else:
        print(f"\n⚠️ Model loading had issues, but may still be usable")
