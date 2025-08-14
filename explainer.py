import sys
import requests

OLLAMA_API = "http://localhost:11434/api/chat"
HEADERS = {"Content-Type": "application/json"}
MODEL = "llama3.2"

def query_ollama(messages):
    """Send messages to Ollama API and return response text."""
    payload = {
        "model": MODEL,
        "messages": messages,
        "stream": False
    }
    try:
        response = requests.post(OLLAMA_API, json=payload, headers=HEADERS)
        return response.json().get("message", {}).get("content", "").strip()
    except Exception as e:
        return f"[Python error] {str(e)}"

def step1_extract_details(output):
    """Step 1: Extract exact factual details without interpretation."""
    extraction_prompt = f"""
From the given command output below, extract all factual details exactly as they appear, in the same order.
Include names, numbers, file paths, dates, times, or other key values without altering or omitting them.
If it is a long list, include the first 5 entries and then state how many more there are.
Do not interpret, summarize, or guess.
Only return the extracted details.

OUTPUT:
{output}
"""
    return query_ollama([{"role": "user", "content": extraction_prompt}])

def step2_explain(query, extracted_details):
    """Step 2: Write a short, accurate explanation using only the extracted details."""
    explanation_prompt = f"""
A user asked: "{query}"
Here are the extracted factual details from the system output:
{extracted_details}

Write a single short sentence explaining the output in plain language for a non-technical user.
Use only the provided extracted details—do not guess or add missing information.
"""
    return query_ollama([{"role": "user", "content": explanation_prompt}])

def main():
    prompt = sys.argv[1] if len(sys.argv) > 1 else ""
    if "~~" not in prompt:
        print("Invalid input format: expected 'query~~output'")
        return

    query, output = prompt.split("~~", 1)
    query, output = query.strip(), output.strip()

    # Step 1: Extract details
    extracted = step1_extract_details(output)

    # Step 2: Explain based on extracted details
    explanation = step2_explain(query, extracted)

    print(explanation or "[Empty response from AI]")

if __name__ == "__main__":
    main()
