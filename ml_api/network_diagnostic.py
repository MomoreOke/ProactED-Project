"""
Network Connectivity Diagnostic Tool for ML API
Tests various aspects of network connectivity to help identify the issue
"""

import socket
import http.server
import socketserver
import threading
import time
import requests
from flask import Flask

def test_port_availability(port):
    """Test if a port is available for binding"""
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            s.bind(('127.0.0.1', port))
            s.listen(1)
            print(f"✅ Port {port} is available for binding")
            return True
    except OSError as e:
        print(f"❌ Port {port} is NOT available: {e}")
        return False

def test_simple_http_server(port):
    """Test a simple HTTP server"""
    try:
        class Handler(http.server.SimpleHTTPRequestHandler):
            def do_GET(self):
                self.send_response(200)
                self.send_header('Content-type', 'text/plain')
                self.end_headers()
                self.wfile.write(b'Hello from test server!')
                
        print(f"🚀 Starting simple HTTP server on port {port}")
        httpd = socketserver.TCPServer(("127.0.0.1", port), Handler)
        
        # Start server in a separate thread
        server_thread = threading.Thread(target=httpd.serve_forever)
        server_thread.daemon = True
        server_thread.start()
        
        # Wait a moment for server to start
        time.sleep(2)
        
        # Test the server
        try:
            response = requests.get(f"http://127.0.0.1:{port}", timeout=5)
            print(f"✅ HTTP server test successful: {response.status_code}")
            print(f"   Response: {response.text}")
            httpd.shutdown()
            return True
        except Exception as e:
            print(f"❌ HTTP server test failed: {e}")
            httpd.shutdown()
            return False
            
    except Exception as e:
        print(f"❌ Could not start HTTP server: {e}")
        return False

def test_flask_minimal(port):
    """Test minimal Flask server"""
    try:
        app = Flask(__name__)
        
        @app.route('/test')
        def test():
            return {'status': 'ok', 'message': 'Flask test successful'}
        
        print(f"🌶️ Testing minimal Flask server on port {port}")
        
        # Start Flask in a thread
        def run_flask():
            app.run(host='127.0.0.1', port=port, debug=False, use_reloader=False)
        
        flask_thread = threading.Thread(target=run_flask)
        flask_thread.daemon = True
        flask_thread.start()
        
        # Wait for Flask to start
        time.sleep(3)
        
        # Test the Flask server
        try:
            response = requests.get(f"http://127.0.0.1:{port}/test", timeout=5)
            print(f"✅ Flask test successful: {response.status_code}")
            print(f"   Response: {response.json()}")
            return True
        except Exception as e:
            print(f"❌ Flask test failed: {e}")
            return False
            
    except Exception as e:
        print(f"❌ Could not start Flask server: {e}")
        return False

def check_windows_firewall():
    """Check Windows Firewall status"""
    import subprocess
    try:
        result = subprocess.run(['netsh', 'advfirewall', 'show', 'allprofiles', 'state'], 
                              capture_output=True, text=True, timeout=10)
        print("🔥 Windows Firewall Status:")
        print(result.stdout)
    except Exception as e:
        print(f"❌ Could not check Windows Firewall: {e}")

def main():
    print("🔍 ML API Connectivity Diagnostic Tool")
    print("=" * 50)
    
    # Test port availability
    ports_to_test = [5001, 5002, 8000]
    available_port = None
    
    for port in ports_to_test:
        if test_port_availability(port):
            available_port = port
            break
    
    if not available_port:
        print("❌ No ports available for testing!")
        return
    
    print(f"\n📡 Using port {available_port} for testing")
    print("-" * 30)
    
    # Test simple HTTP server
    print("\n1️⃣ Testing Simple HTTP Server:")
    test_simple_http_server(available_port)
    
    # Wait between tests
    time.sleep(2)
    
    # Test Flask
    print("\n2️⃣ Testing Minimal Flask Server:")
    test_flask_minimal(available_port)
    
    # Check Windows Firewall
    print("\n3️⃣ Checking Windows Firewall:")
    check_windows_firewall()
    
    print("\n🏁 Diagnostic complete!")

if __name__ == "__main__":
    main()
