import sys
import os
import requests
import ollama
from bs4 import BeautifulSoup
from IPython.display import Markdown, display


def main():
    prompt = sys.argv[1] if len(sys.argv) > 1 else ""
    if "~~" not in prompt:
        print("Invalid input format: expected 'query~~output'")
        return

    query, output = prompt.split("~~", 1)
    query = query.strip()
    output = output.strip()
    OLLAMA_API = "http://localhost:11434/api/chat"
    HEADERS = {"Content-Type": "application/json"}
    MODEL = "llama3.2"

    messages = [
        {
            "role": "user",
        "content": f"""
        You are a helpful assistant. A user asked this question: "{query}".
        The raw system output is: "{output}".
        Explain this output in a short, simple sentence that a normal user can understand.
        Do not guess; explain only based on this output and the query.Explain this output in a short, simple sentence that a normal user can understand.
        """
        }
    ]

    payload = {
        "model": MODEL,
        "messages": messages,
        "stream": False
    }

    try:
        response = requests.post(OLLAMA_API, json=payload, headers=HEADERS)
        result = response.json().get("message", {}).get("content", "").strip()
        print(result or "[Empty response from AI]")
    except Exception as e:
        print(f"[Python error] {str(e)}")

if __name__ == "__main__":
    main()
