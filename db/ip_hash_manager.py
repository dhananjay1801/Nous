import hashlib
import mysql.connector
from datetime import datetime
import sys

DB_CONFIG = {
    'user': 'root',     
    'password': 'admin', 
    'host': 'localhost',
    'database': 'project'
}

def generate_hash(ip):
    current_hour = datetime.utcnow().strftime('%Y-%m-%d-%H')
    hash_input = f"{ip}-{current_hour}"
    return hashlib.sha256(hash_input.encode()).hexdigest()[:8].upper()

def insert_ip(ip):
    hashcode = generate_hash(ip)
    conn = mysql.connector.connect(**DB_CONFIG)
    cursor = conn.cursor()
    try:
        cursor.execute("INSERT INTO ip_hash (ip_address, hashcode) VALUES (%s, %s) ON DUPLICATE KEY UPDATE hashcode=%s", (ip, hashcode, hashcode))
        conn.commit()
        print(f"Inserted/Updated IP: {ip} with hashcode: {hashcode}")
    except mysql.connector.Error as err:
        print(f"Error: {err}")
    finally:
        cursor.close()
        conn.close()

def insert_ips(ip_list):
    conn = mysql.connector.connect(**DB_CONFIG)
    cursor = conn.cursor()
    try:
        for ip in ip_list:
            hashcode = generate_hash(ip)
            cursor.execute("INSERT INTO ip_hash (ip_address, hashcode) VALUES (%s, %s) ON DUPLICATE KEY UPDATE hashcode=%s", (ip, hashcode, hashcode))
            print(f"Inserted/Updated IP: {ip} with hashcode: {hashcode}")
        conn.commit()
    except mysql.connector.Error as err:
        print(f"Error: {err}")
    finally:
        cursor.close()
        conn.close()

def update_hashcodes():
    conn = mysql.connector.connect(**DB_CONFIG)
    cursor = conn.cursor()
    try:
        cursor.execute("SELECT ip_address FROM ip_hash")
        ips = cursor.fetchall()
        for (ip,) in ips:
            new_hash = generate_hash(ip)
            cursor.execute("UPDATE ip_hash SET hashcode=%s WHERE ip_address=%s", (new_hash, ip))
        conn.commit()
        print("All hashcodes updated.")
    except mysql.connector.Error as err:
        print(f"Error: {err}")
    finally:
        cursor.close()
        conn.close()

def main():
    if len(sys.argv) < 2:
        print("Usage: python ip_hash_manager.py [insert <ip_address> ... | update]")
        return
    command = sys.argv[1]
    if command == 'insert' and len(sys.argv) >= 3:
        if len(sys.argv) == 3:
            insert_ip(sys.argv[2])
        else:
            insert_ips(sys.argv[2:])
    elif command == 'update':
        update_hashcodes()
    else:
        print("Invalid command or arguments.")

if __name__ == "__main__":
    main() 