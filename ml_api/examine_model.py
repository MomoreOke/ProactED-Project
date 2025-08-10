import pickle
import pandas as pd
import numpy as np

# Load the model
with open('complete_equipment_failure_prediction_system.pkl', 'rb') as f:
    model_system = pickle.load(f)

print('=== MODEL SYSTEM STRUCTURE ===')
print(f'Type: {type(model_system)}')
if isinstance(model_system, dict):
    print(f'Keys: {list(model_system.keys())}')
else:
    print('Not a dictionary')

if isinstance(model_system, dict) and 'model_info' in model_system:
    model_info = model_system['model_info']
    print('\n=== MODEL INFO ===')
    print(f'Keys: {list(model_info.keys())}')
    print(f'Model name: {model_info.get("model_name", "Unknown")}')
    print(f'Features: {model_info.get("features", [])}')
    print(f'Threshold: {model_info.get("optimal_threshold", "Unknown")}')
    
    if 'performance_metrics' in model_info:
        metrics = model_info['performance_metrics']
        print(f'\n=== PERFORMANCE METRICS ===')
        for key, value in metrics.items():
            print(f'{key}: {value}')
    
    # Check if model object exists
    if 'model_object' in model_info:
        model = model_info['model_object']
        print(f'\n=== MODEL OBJECT ===')
        print(f'Type: {type(model)}')
        if hasattr(model, 'feature_importances_'):
            print(f'Feature importances: {model.feature_importances_}')
        if hasattr(model, 'n_estimators'):
            print(f'Number of estimators: {model.n_estimators}')

# Check if scaler exists
if isinstance(model_system, dict) and 'scaler' in model_system:
    scaler = model_system['scaler']
    print(f'\n=== SCALER ===')
    print(f'Type: {type(scaler)}')

print('\n=== SAMPLE PREDICTION TEST ===')
# Test with sample data
if isinstance(model_system, dict) and 'model_info' in model_system:
    model_info = model_system['model_info']
    features = model_info.get('features', [])
    model = model_info.get('model_object')
    
    if model and features:
        # Create sample data
        sample_data = {
            'age_months': 24,
            'operating_temperature': 75.0,
            'vibration_level': 3.2,
            'power_consumption': 850.0,
            'humidity_level': 45.0,
            'dust_accumulation': 2.5,
            'performance_score': 0.85,
            'daily_usage_hours': 8.0
        }
        
        # Create DataFrame
        df = pd.DataFrame([sample_data])
        
        # Ensure all features are present
        for feature in features:
            if feature not in df.columns:
                df[feature] = 0
        
        X = df[features]
        print(f'Sample input shape: {X.shape}')
        print(f'Sample input:\n{X}')
        
        # Apply scaling if available
        if 'scaler' in model_system:
            X_scaled = model_system['scaler'].transform(X)
            print(f'Scaled input:\n{X_scaled}')
            prediction = model.predict(X_scaled)[0]
        else:
            prediction = model.predict(X)[0]
        
        print(f'Sample prediction: {prediction:.3f}')
