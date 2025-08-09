"""
Enhanced Equipment Prediction API for .NET Integration
Fallback implementation for testing when the full API is not available
"""

from flask import Flask, request, jsonify
from flask_cors import CORS
import datetime
import random
import logging

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = Flask(__name__)
CORS(app)  # Enable CORS for .NET integration

@app.route('/', methods=['GET'])
def api_documentation():
    """API documentation and endpoint list"""
    return jsonify({
        "success": True,
        "message": "Enhanced Equipment Prediction API for .NET Integration",
        "version": "1.0.0",
        "timestamp": datetime.datetime.utcnow().isoformat(),
        "endpoints": {
            "GET /": "API documentation and endpoint list",
            "GET /api/health": "Health check",
            "GET /api/model/info": "Model information",
            "POST /api/equipment/predict": "Single equipment prediction",
            "POST /api/equipment/batch-predict": "Batch equipment prediction"
        }
    })

@app.route('/api/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({
        "status": "healthy",
        "timestamp": datetime.datetime.utcnow().isoformat(),
        "version": "1.0.0",
        "model_loaded": True,
        "model_type": "Fallback Test Model"
    })

@app.route('/api/model/info', methods=['GET'])
def model_info():
    """Get model information"""
    return jsonify({
        "success": True,
        "model_version": "Test-Model-v1.0",
        "training_date": "2025-08-07T10:00:00Z",
        "accuracy": 0.887,
        "features": [
            "age_months",
            "operating_temperature", 
            "vibration_level",
            "power_consumption"
        ],
        "description": "Fallback test model for ProactED integration testing"
    })

@app.route('/api/equipment/predict', methods=['POST'])
def predict_single():
    """Predict failure for a single equipment"""
    try:
        data = request.get_json()
        logger.info(f"Received prediction request for equipment: {data.get('equipment_id')}")
        
        # Validate required fields
        required_fields = ['equipment_id', 'age_months', 'operating_temperature', 'vibration_level', 'power_consumption']
        if not all(field in data for field in required_fields):
            return jsonify({
                "success": False,
                "error": f"Missing required fields. Required: {required_fields}"
            }), 400
        
        # Generate realistic prediction based on input data
        age_months = data['age_months']
        temp = data['operating_temperature']
        vibration = data['vibration_level']
        power = data['power_consumption']
        
        # Simple risk calculation based on inputs
        risk_score = 0.0
        risk_score += min(age_months / 100.0, 0.4)  # Age factor (max 40%)
        risk_score += min((temp - 50) / 100.0, 0.3) if temp > 50 else 0  # Temperature factor
        risk_score += min(vibration / 10.0, 0.2)  # Vibration factor  
        risk_score += min(power / 2000.0, 0.1)  # Power factor
        
        # Add some randomness
        risk_score += random.uniform(-0.1, 0.1)
        risk_score = max(0.01, min(0.99, risk_score))
        
        # Determine risk level
        if risk_score >= 0.7:
            risk_level = "Critical"
        elif risk_score >= 0.5:
            risk_level = "High"
        elif risk_score >= 0.3:
            risk_level = "Medium"
        else:
            risk_level = "Low"
        
        # Generate confidence score
        confidence_score = 0.85 + random.uniform(-0.15, 0.1)
        confidence_score = max(0.6, min(0.95, confidence_score))
        
        prediction = {
            "success": True,
            "equipment_id": data['equipment_id'],
            "failure_probability": round(risk_score, 3),
            "risk_level": risk_level,
            "confidence_score": round(confidence_score, 3),
            "prediction_timestamp": datetime.datetime.utcnow().isoformat(),
            "model_version": "Test-Model-v1.0"
        }
        
        logger.info(f"Generated prediction for {data['equipment_id']}: {risk_level} risk ({risk_score:.1%})")
        return jsonify(prediction)
        
    except Exception as e:
        logger.error(f"Prediction error: {e}")
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/api/equipment/batch-predict', methods=['POST'])
def predict_batch():
    """Predict failure for multiple equipment items"""
    try:
        data = request.get_json()
        
        if 'equipment_list' not in data:
            return jsonify({
                "success": False,
                "error": "Missing 'equipment_list' field"
            }), 400
        
        equipment_list = data['equipment_list']
        predictions = []
        
        logger.info(f"Processing batch prediction for {len(equipment_list)} equipment items")
        
        for equipment_data in equipment_list:
            try:
                # Validate each equipment item
                required_fields = ['equipment_id', 'age_months', 'operating_temperature', 'vibration_level', 'power_consumption']
                if not all(field in equipment_data for field in required_fields):
                    predictions.append({
                        "success": False,
                        "equipment_id": equipment_data.get('equipment_id', 'unknown'),
                        "error": "Missing required fields"
                    })
                    continue
                
                # Generate prediction (same logic as single prediction)
                age_months = equipment_data['age_months']
                temp = equipment_data['operating_temperature']
                vibration = equipment_data['vibration_level']
                power = equipment_data['power_consumption']
                
                # Simple risk calculation
                risk_score = 0.0
                risk_score += min(age_months / 100.0, 0.4)
                risk_score += min((temp - 50) / 100.0, 0.3) if temp > 50 else 0
                risk_score += min(vibration / 10.0, 0.2)
                risk_score += min(power / 2000.0, 0.1)
                risk_score += random.uniform(-0.1, 0.1)
                risk_score = max(0.01, min(0.99, risk_score))
                
                # Determine risk level
                if risk_score >= 0.7:
                    risk_level = "Critical"
                elif risk_score >= 0.5:
                    risk_level = "High"
                elif risk_score >= 0.3:
                    risk_level = "Medium"
                else:
                    risk_level = "Low"
                
                confidence_score = 0.85 + random.uniform(-0.15, 0.1)
                confidence_score = max(0.6, min(0.95, confidence_score))
                
                prediction = {
                    "success": True,
                    "equipment_id": equipment_data['equipment_id'],
                    "failure_probability": round(risk_score, 3),
                    "risk_level": risk_level,
                    "confidence_score": round(confidence_score, 3),
                    "prediction_timestamp": datetime.datetime.utcnow().isoformat(),
                    "model_version": "Test-Model-v1.0"
                }
                predictions.append(prediction)
                
            except Exception as e:
                predictions.append({
                    "success": False,
                    "equipment_id": equipment_data.get('equipment_id', 'unknown'),
                    "error": str(e)
                })
        
        return jsonify({
            "success": True,
            "processed_count": len(predictions),
            "predictions": predictions
        })
        
    except Exception as e:
        logger.error(f"Batch prediction error: {e}")
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.errorhandler(404)
def not_found(error):
    return jsonify({
        "success": False,
        "error": "Endpoint not found",
        "available_endpoints": ["/", "/api/health", "/api/model/info", "/api/equipment/predict", "/api/equipment/batch-predict"]
    }), 404

@app.errorhandler(500)
def internal_error(error):
    return jsonify({
        "success": False,
        "error": "Internal server error"
    }), 500

if __name__ == '__main__':
    print("🚀 Starting ProactED Test ML API")
    print("📡 API Endpoints:")
    print("   GET  /                              - API documentation")
    print("   GET  /api/health                    - Health check")
    print("   GET  /api/model/info                - Model information")
    print("   POST /api/equipment/predict         - Single equipment prediction")
    print("   POST /api/equipment/batch-predict   - Batch equipment predictions")
    print("🌐 Server starting on http://localhost:5000")
    print("🔗 Ready for .NET integration testing!")
    
    app.run(host='0.0.0.0', port=5000, debug=True)
