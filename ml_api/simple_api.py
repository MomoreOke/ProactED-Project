"""
Simplified Flask API for ProactED ML predictions
Minimal version to test API connectivity
"""

from flask import Flask, jsonify, request
from flask_cors import CORS
import json

app = Flask(__name__)
CORS(app)

@app.route('/api/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({
        'status': 'healthy',
        'timestamp': '2025-08-09T12:00:00Z',
        'model_version': 'Random Forest v1.0',
        'api_version': '1.0',
        'model_accuracy': 0.911,
        'features_count': 8,
        'model_type': 'RandomForestClassifier'
    })

@app.route('/api/model/info', methods=['GET'])
def model_info():
    """Model information endpoint"""
    return jsonify({
        'model_name': 'Equipment Failure Prediction Model',
        'model_type': 'Random Forest',
        'version': '1.0',
        'accuracy': 0.911,
        'features': [
            'age_months',
            'operating_temperature',
            'vibration_level',
            'power_consumption',
            'maintenance_history',
            'usage_hours',
            'environmental_factors',
            'component_health'
        ],
        'last_trained': '2025-08-09T00:00:00Z',
        'training_samples': 10000
    })

@app.route('/api/equipment/predict', methods=['POST'])
def predict_equipment():
    """Single equipment prediction endpoint"""
    try:
        data = request.get_json()
        equipment_id = data.get('equipment_id', 'unknown')
        
        # Simple mock prediction based on input data
        age = data.get('age_months', 24)
        temp = data.get('operating_temperature', 65)
        vibration = data.get('vibration_level', 2.5)
        power = data.get('power_consumption', 250)
        
        # Calculate risk based on input parameters
        risk_score = 0.0
        risk_score += min(age / 100.0, 0.3)  # Age factor
        risk_score += min((temp - 20) / 100.0, 0.3)  # Temperature factor
        risk_score += min(vibration / 10.0, 0.2)  # Vibration factor
        risk_score += min(power / 1000.0, 0.2)  # Power factor
        
        # Add some randomization
        import random
        risk_score += random.uniform(-0.1, 0.1)
        risk_score = max(0.0, min(1.0, risk_score))
        
        # Determine risk level
        if risk_score >= 0.7:
            risk_level = "Critical"
            risk_color = "danger"
        elif risk_score >= 0.5:
            risk_level = "High"
            risk_color = "warning"
        elif risk_score >= 0.3:
            risk_level = "Medium" 
            risk_color = "info"
        else:
            risk_level = "Low"
            risk_color = "success"
        
        return jsonify({
            'success': True,
            'equipment_id': equipment_id,
            'failure_probability': round(risk_score, 3),
            'risk_level': risk_level,
            'confidence_score': round(random.uniform(0.8, 0.95), 3),
            'model_version': 'Random Forest v1.0',
            'prediction_timestamp': '2025-08-09T12:00:00Z',
            'risk_color': risk_color,
            'factors': {
                'age_impact': round(min(age / 100.0, 0.3), 3),
                'temperature_impact': round(min((temp - 20) / 100.0, 0.3), 3),
                'vibration_impact': round(min(vibration / 10.0, 0.2), 3),
                'power_impact': round(min(power / 1000.0, 0.2), 3)
            }
        })
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e),
            'equipment_id': data.get('equipment_id', 'unknown') if 'data' in locals() else 'unknown'
        }), 500

@app.route('/api/equipment/batch-predict', methods=['POST'])
def batch_predict():
    """Batch prediction endpoint"""
    try:
        data = request.get_json()
        equipment_list = data.get('equipment_list', [])
        
        predictions = []
        for equipment in equipment_list:
            # Reuse single prediction logic
            single_request = {'json': equipment}
            with app.test_request_context('/api/equipment/predict', method='POST', json=equipment):
                response = predict_equipment()
                if hasattr(response, 'json'):
                    predictions.append(response.json)
                else:
                    predictions.append(response[0].json)
        
        return jsonify({
            'success': True,
            'predictions': predictions,
            'total_processed': len(predictions),
            'batch_timestamp': '2025-08-09T12:00:00Z'
        })
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e),
            'total_processed': 0
        }), 500

if __name__ == '__main__':
    print("🚀 Starting Simplified ProactED ML API")
    print("📡 Available endpoints:")
    print("   GET  /api/health")
    print("   GET  /api/model/info") 
    print("   POST /api/equipment/predict")
    print("   POST /api/equipment/batch-predict")
    print("🔗 Server running on http://localhost:5001")
    
    try:
        app.run(host='127.0.0.1', port=5001, debug=False, threaded=True)
    except Exception as e:
        print(f"❌ Error starting Flask server: {e}")
        import traceback
        traceback.print_exc()
        input("Press Enter to continue...")
