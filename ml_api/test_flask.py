from flask import Flask

app = Flask(__name__)

@app.route('/test')
def test():
    return "Flask is working!"

if __name__ == '__main__':
    print("Starting simple Flask test...")
    try:
        app.run(host='127.0.0.1', port=5002, debug=True)
    except Exception as e:
        print(f"Error: {e}")
        import traceback
        traceback.print_exc()
