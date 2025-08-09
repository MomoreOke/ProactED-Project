"""
Ultra-minimal Flask API for testing - no dependencies
"""

try:
    from flask import Flask, jsonify
    print("✅ Flask import successful")
except ImportError as e:
    print(f"❌ Flask import failed: {e}")
    exit(1)

try:
    from flask_cors import CORS
    print("✅ Flask-CORS import successful")
    cors_available = True
except ImportError as e:
    print(f"⚠️ Flask-CORS import failed: {e}")
    print("   Continuing without CORS...")
    cors_available = False

app = Flask(__name__)

if cors_available:
    CORS(app)

@app.route('/test')
def test():
    return jsonify({'status': 'ok', 'message': 'Minimal Flask API working!'})

@app.route('/health')
def health():
    return jsonify({
        'status': 'healthy',
        'server': 'minimal-flask'
    })

if __name__ == '__main__':
    print("🧪 Starting Ultra-Minimal Flask API Test")
    print("📍 Endpoints:")
    print("   GET /test")
    print("   GET /health")
    
    try:
        print("🚀 Starting server...")
        app.run(host='127.0.0.1', port=5001, debug=False)
        print("✅ Server started successfully")
    except Exception as e:
        print(f"❌ Server failed to start: {e}")
        import traceback
        traceback.print_exc()
    finally:
        print("🏁 Server stopped")
