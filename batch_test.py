import csv
import subprocess
import os

# Paths to your existing programs
BACKEND_PATH = r"D:\Project stuff\Nous\backend.py"
SANITIZER_PATH = r"D:\Project stuff\Nous\Sanitizer.py"

CSV_FILE = r"D:\Project stuff\Nous\Utils\cmd_prompts.csv"  # update if needed
OUTPUT_FILE = r"results.txt"

def run_backend(prompt):
    """Run backend.py with given prompt and return generated command"""
    try:
        result = subprocess.run(
            ["python", BACKEND_PATH, prompt],
            capture_output=True,
            text=True,
            timeout=30
        )
        return result.stdout.strip()
    except Exception as e:
        return f"[ERROR in backend] {e}"

def run_sanitizer(command):
    """Run Sanitizer.py on generated command and return sanitized result"""
    try:
        result = subprocess.run(
            ["python", SANITIZER_PATH, command],
            capture_output=True,
            text=True,
            timeout=10
        )
        return result.stdout.strip()
    except Exception as e:
        return f"[ERROR in sanitizer] {e}"

def main():
    with open(CSV_FILE, newline="", encoding="utf-8") as csvfile, open(OUTPUT_FILE, "w", encoding="utf-8") as outfile:
        reader = csv.DictReader(csvfile)
        for idx, row in enumerate(reader, start=1):
            prompt = row["Prompt"]
            expected = row["ExpectedCommand"]

            generated = run_backend(prompt)
            sanitized = run_sanitizer(generated)
            print(f"Test {idx}")
            print(f"Prompt: {prompt}")
            print(f"Expected: {expected}")
            print(f"Generated: {generated}")
            print(f"Sanitized: {sanitized}\n")
        

           
            outfile.write(f"=== Test {idx} ===\n")
            outfile.write(f"Prompt: {prompt}\n")
            outfile.write(f"Expected: {expected}\n")
            outfile.write(f"Generated: {generated}\n")
            outfile.write(f"Sanitized: {sanitized}\n")
            outfile.write("\n")

    print(f"Testing completed. Results saved to {OUTPUT_FILE}")

if __name__ == "__main__":
    main()
