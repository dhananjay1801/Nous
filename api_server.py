from flask import Flask, request, jsonify
from flask_cors import CORS
import mysql.connector
import hashlib
from datetime import datetime
from Utils.Logger import Logger

# Database configuration
DB_CONFIG = {
    'host': 'localhost',
    'user': 'root',
    'password': 'admin',  # Change this to your MySQL password
    'database': 'project'
}

app = Flask(__name__)
CORS(app)

@app.route('/register_ip', methods=['POST'])
def register_ip():
    try:
        data = request.get_json()
        ip_address = data.get('ip_address')
        if not ip_address:
            Logger.EWrite("IP address missing in request")
            return jsonify({'error': 'IP address is required'}), 400

        # Generate hashcode (C# compatible: first 4 bytes of SHA256(ip-YYYY-MM-DD-HH))
        current_hour = datetime.utcnow().strftime('%Y-%m-%d-%H')
        hash_input = f"{ip_address}-{current_hour}"
        hash_bytes = hashlib.sha256(hash_input.encode()).digest()
        hashcode = ''.join(f"{hash_bytes[i]:02x}" for i in range(4)).upper()

        # Insert or update the IP address
        conn = mysql.connector.connect(**DB_CONFIG)
        cursor = conn.cursor()
        query = """
        INSERT INTO ip_hash (ip_address, hashcode)
        VALUES (%s, %s)
        ON DUPLICATE KEY UPDATE hashcode=VALUES(hashcode), last_updated=CURRENT_TIMESTAMP
        """
        cursor.execute(query, (ip_address, hashcode))
        conn.commit()
        cursor.close()
        conn.close()

        Logger.SWrite(f"Registered IP: {ip_address} with hashcode: {hashcode}")
        return jsonify({'success': True, 'ip_address': ip_address, 'hashcode': hashcode})

    except Exception as e:
        Logger.EWrite(f"Error registering IP: {e}")
        return jsonify({'error': str(e)}), 500

@app.route('/health', methods=['GET'])
def health():
    Logger.SWrite("Health check requested")
    return jsonify({'status': 'healthy'})

if __name__ == '__main__':
    Logger.SWrite("Starting IP Registration API Server...")
    app.run(host='0.0.0.0', port=5000, debug=False)
