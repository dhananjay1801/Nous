import sys
import yaml
import os

AVOIDED_FILE = "data\\Avoided.yml"

def load_avoided_commands():
    if not os.path.exists(AVOIDED_FILE):
        print(f"echo [!] Avoided.yml not found at {AVOIDED_FILE}")
        return []
    
    with open(AVOIDED_FILE, "r", encoding="utf-8") as f:
        try:
            data = yaml.safe_load(f)
            return data.get("dangerous_commands", [])
        except yaml.YAMLError as e:
            print(f"echo [!] Error parsing Avoided.yml: {e}")
            return []

def check_command(command):
    avoided = load_avoided_commands()
    for bad_cmd in avoided:
        if bad_cmd.lower() in command.lower():
            print("echo [-] MALICIOUS COMMAND DETECTED. NOT RUNNING THE COMMAND")
            sys.exit(1)  # Non-zero exit to indicate failure
    print(command) 

def main():
    if len(sys.argv) < 2:
        print("echo [!] No command provided to sanitizer.")
        sys.exit(1)
    
    generated_command = sys.argv[1]
    check_command(generated_command)

if __name__ == "__main__":
    main()
