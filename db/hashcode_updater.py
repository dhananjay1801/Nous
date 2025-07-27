import time
from ip_hash_manager import update_hashcodes

if __name__ == "__main__":
    while True:
        try:
            print("Updating hashcodes...")
            update_hashcodes()
            print("Update complete. Waiting for 1 hour.")
        except Exception as e:
            print(f"Error during update: {e}")
        time.sleep(3600)