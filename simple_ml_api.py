"""
Simplified ML API for ProactED Integration Testing
This version doesn't require the pickle file and provides realistic predictions
"""

from flask import Flask, request, jsonify
import datetime
import random
import logging
import json

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = Flask(__name__)

# Simple in-memory model simulation
class SimpleEquipmentPredictor:
    def __init__(self):
        self.model_version = "Simple-Test-Model-v1.0"
        self.accuracy = 0.887
        random.seed(42)  # For consistent results during testing
        
    def predict(self, equipment_data):
        """Generate realistic failure probability based on equipment parameters"""
        try:
            # Extract parameters
            age_months = float(equipment_data.get('age_months', 0))
            temp = float(equipment_data.get('operating_temperature', 50))
            vibration = float(equipment_data.get('vibration_level', 1))
            power = float(equipment_data.get('power_consumption', 1000))
            
            # Calculate risk score based on realistic factors
            risk_score = 0.0
            
            # Age factor (older equipment more likely to fail)
            risk_score += min(age_months / 120.0, 0.4)  # Max 40% from age
            
            # Temperature factor (high temp = higher risk)
            if temp > 80:
                risk_score += min((temp - 80) / 50.0, 0.25)  # Max 25% from temp
            elif temp > 60:
                risk_score += min((temp - 60) / 100.0, 0.15)  # Max 15% from moderate temp
                
            # Vibration factor (high vibration = problems)
            if vibration > 5.0:
                risk_score += min((vibration - 5.0) / 10.0, 0.2)  # Max 20% from vibration
            elif vibration > 3.0:
                risk_score += min((vibration - 3.0) / 20.0, 0.1)  # Max 10% from moderate vibration
                
            # Power consumption factor (unusual power = potential issues)
            normal_power = 1500  # Assume normal power consumption
            power_deviation = abs(power - normal_power) / normal_power
            risk_score += min(power_deviation * 0.15, 0.15)  # Max 15% from power deviation
            
            # Add some realistic randomness
            random_factor = random.uniform(-0.05, 0.05)
            risk_score += random_factor
            
            # Ensure probability is in valid range
            failure_probability = max(0.01, min(0.99, risk_score))
            
            # Determine risk level
            if failure_probability >= 0.7:
                risk_level = "Critical"
            elif failure_probability >= 0.5:
                risk_level = "High"  
            elif failure_probability >= 0.3:
                risk_level = "Medium"
            else:
                risk_level = "Low"
                
            # Calculate confidence (inversely related to uncertainty factors)
            confidence = 0.85
            if age_months < 6:  # New equipment harder to predict
                confidence -= 0.1
            if temp > 90 or temp < 10:  # Extreme temperatures harder to predict
                confidence -= 0.1
            if vibration > 8:  # Very high vibration harder to predict
                confidence -= 0.05
                
            confidence_score = max(0.6, min(0.95, confidence + random.uniform(-0.05, 0.05)))
            
            return {
                "success": True,
                "failure_probability": round(failure_probability, 3),
                "risk_level": risk_level,
                "confidence_score": round(confidence_score, 3),
                "model_version": self.model_version,
                "prediction_timestamp": datetime.datetime.utcnow().isoformat()
            }
            
        except Exception as e:
            logger.error(f"Prediction error: {e}")
            return {
                "success": False,
                "error": str(e),
                "failure_probability": 0.5,
                "risk_level": "Unknown",
                "confidence_score": 0.0
            }

# Initialize the predictor
predictor = SimpleEquipmentPredictor()

@app.route('/', methods=['GET'])
def api_documentation():
    """API documentation"""
    return jsonify({
        "success": True,
        "message": "ProactED Simplified ML API for Integration Testing",
        "version": "1.0.0",
        "timestamp": datetime.datetime.utcnow().isoformat(),
        "endpoints": {
            "GET /": "API documentation",
            "GET /api/health": "Health check",
            "GET /api/model/info": "Model information", 
            "POST /api/equipment/predict": "Single equipment prediction",
            "POST /api/equipment/batch-predict": "Batch equipment predictions"
        },
        "note": "This is a simplified test API that doesn't require the ML model pickle file"
    })

@app.route('/api/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({
        "status": "healthy",
        "timestamp": datetime.datetime.utcnow().isoformat(),
        "version": "1.0.0",
        "model_loaded": True,
        "model_type": "Simple Test Predictor",
        "message": "API is running and ready for predictions"
    })

@app.route('/api/model/info', methods=['GET'])
def model_info():
    """Get model information"""
    return jsonify({
        "success": True,
        "model_version": predictor.model_version,
        "training_date": "2025-08-07T10:00:00Z",
        "accuracy": predictor.accuracy,
        "features": [
            "age_months",
            "operating_temperature",
            "vibration_level", 
            "power_consumption"
        ],
        "description": "Simplified test model for ProactED integration testing"
    })

@app.route('/api/equipment/predict', methods=['POST'])
def predict_single():
    """Predict failure for a single equipment"""
    try:
        data = request.get_json()
        
        if not data:
            return jsonify({
                "success": False,
                "error": "No JSON data provided"
            }), 400
            
        logger.info(f"Received single prediction request for: {data.get('equipment_id')}")
        
        # Validate required fields
        required_fields = ['equipment_id', 'age_months', 'operating_temperature', 'vibration_level', 'power_consumption']
        missing_fields = [field for field in required_fields if field not in data]
        
        if missing_fields:
            return jsonify({
                "success": False,
                "error": f"Missing required fields: {missing_fields}"
            }), 400
        
        # Generate prediction
        prediction = predictor.predict(data)
        prediction["equipment_id"] = data['equipment_id']
        
        logger.info(f"Generated prediction for {data['equipment_id']}: {prediction['risk_level']} risk")
        
        return jsonify(prediction)
        
    except Exception as e:
        logger.error(f"Single prediction error: {e}")
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/api/equipment/batch-predict', methods=['POST'])
def predict_batch():
    """Predict failure for multiple equipment items"""
    try:
        data = request.get_json()
        
        if not data or 'equipment_list' not in data:
            return jsonify({
                "success": False,
                "error": "Missing 'equipment_list' field"
            }), 400
        
        equipment_list = data['equipment_list']
        logger.info(f"Processing batch prediction for {len(equipment_list)} items")
        
        predictions = []
        
        for equipment_data in equipment_list:
            try:
                # Validate each equipment item
                required_fields = ['equipment_id', 'age_months', 'operating_temperature', 'vibration_level', 'power_consumption']
                missing_fields = [field for field in required_fields if field not in equipment_data]
                
                if missing_fields:
                    predictions.append({
                        "success": False,
                        "equipment_id": equipment_data.get('equipment_id', 'unknown'),
                        "error": f"Missing fields: {missing_fields}"
                    })
                    continue
                
                # Generate prediction
                prediction = predictor.predict(equipment_data)
                prediction["equipment_id"] = equipment_data['equipment_id']
                predictions.append(prediction)
                
            except Exception as e:
                predictions.append({
                    "success": False,
                    "equipment_id": equipment_data.get('equipment_id', 'unknown'),
                    "error": str(e)
                })
        
        logger.info(f"Completed batch prediction: {len(predictions)} results")
        
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
    print("🚀 Starting ProactED Simplified ML API")
    print("📡 API Endpoints:")
    print("   GET  /                              - API documentation")
    print("   GET  /api/health                    - Health check")
    print("   GET  /api/model/info                - Model information")
    print("   POST /api/equipment/predict         - Single equipment prediction") 
    print("   POST /api/equipment/batch-predict   - Batch equipment predictions")
    print("🌐 Server starting on http://localhost:5000")
    print("🔗 Ready for .NET ProactED integration testing!")
    print("💡 This simplified version doesn't require the ML model pickle file")
    print("")
    
    app.run(host='0.0.0.0', port=5000, debug=True)
