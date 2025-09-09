import sys
import os
from pathlib import Path
import requests
from dotenv import load_dotenv
from langchain_community.vectorstores import FAISS
from langchain_google_genai import GoogleGenerativeAIEmbeddings, ChatGoogleGenerativeAI
#from Utils. import 
from datetime import datetime

# -----------------------------------------------------------
# Logging setup (uses BASE_PATH from .env if available)
# -----------------------------------------------------------
BASE_PATH = Path(os.getenv("BASE_PATH", Path(__file__).resolve().parent))
LOG_PATH = str(BASE_PATH / "Log.txt")

def _ensure_log_file():
    """Ensure that the log directory and file exist."""
    try:
        directory = os.path.dirname(LOG_PATH)
        if directory and not os.path.exists(directory):
            os.makedirs(directory, exist_ok=True)
        if not os.path.exists(LOG_PATH):
            # create an empty log file
            open(LOG_PATH, "a", encoding="utf-8").close()
    except Exception:
        # Best-effort - swallow errors (consistent with original intent)
        pass

def SWrite(message: str):
    """Success/info log: [+] [dd/MM/yyyy HH:mm:ss] message"""
    try:
        _ensure_log_file()
        stamp = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
        with open(LOG_PATH, "a", encoding="utf-8") as f:
            f.write(f"[+] [{stamp}] {message}\n")
    except Exception as ex:
        # swallow/log to console only
        try:
            print(f" SWrite Error: {ex}")
        except Exception:
            pass

def EWrite(message: str):
    """Error log: [-] [dd/MM/yyyy HH:mm:ss] message"""
    try:
        _ensure_log_file()
        stamp = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
        with open(LOG_PATH, "a", encoding="utf-8") as f:
            f.write(f"[-] [{stamp}] {message}\n")
    except Exception as ex:
        try:
            print(f" EWrite Error: {ex}")
        except Exception:
            pass

# -----------------------------------------------------------
# End logging setup
# -----------------------------------------------------------

load_dotenv()
SWrite("Environment variables loaded via dotenv.")

GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")
LLM_PROVIDER = "gemini"
SWrite(f"LLM provider set to '{LLM_PROVIDER}'. Initializing components.")

def call_gemini_api(messages):
    try:
        SWrite("Calling Gemini API (gemini-2.0-flash).")
        llm = ChatGoogleGenerativeAI(model="gemini-2.0-flash", temperature=0.0, google_api_key=GEMINI_API_KEY)
        response = llm.invoke(messages)
        # Truncate to avoid huge log lines
        prev = getattr(response, "content", "")
        SWrite(f"Gemini API call succeeded. Response length: {len(prev) if prev else 0}.")
        return response.content
    except Exception as e:
        EWrite(f"Error contacting Gemini API: {e}")
        return f"Error contacting Gemini API: {e}"

'''
def query_ollama(messages):
    """Send messages to Ollama API and return response text."""
    payload = {
        "model": MODEL,
        "messages": messages,
        "stream": False
    }
    try:
        response = requests.post(OLLAMA_API, json=payload, headers=HEADERS)
        return response.json().get("message", {}).get("content", "")Strip()
    except Exception as e:
        return f"[Python error] {str(e)}" 
'''

def step1_extract_details(output):
    """Step 1: Extract exact factual details without interpretation."""
    SWrite(f"Step 1 start. Output length: {len(output)}.")
    extraction_prompt = f"""
From the given command output below, extract all factual details exactly as they appear, in the same order.
Include names, numbers, file paths, dates, times, or other key values without altering or omitting them.
If it is a long list, include everything.
Do not interpret, summarize, or guess.
Only return the extracted details.
OUTPUT:
{output}
"""
    result = call_gemini_api([{"role": "user", "content": extraction_prompt}])
    SWrite(f"Step 1 completed. Extracted length: {len(result) if result else 0}.")
    return result

def step2_explain(query, extracted_details):
    """Step 2: Write an accurate explanation using only the extracted details."""
    SWrite(f"Step 2 start. Query: {query[:120]}")
    explanation_prompt = f"""
A user asked: "{query}"
Here are the extracted factual details from the system output:
{extracted_details}


Write an answer to the user’s question directly using only the extracted details. 
List all program names directly without summarizing if asked. 
Do not shorten the list.
If many programs exist, output all of them separated by commas or line breaks.Do not guess or add missing information.

"""
    result = call_gemini_api([{"role": "user", "content": explanation_prompt}])
    SWrite(f"Step 2 completed. Explanation length: {len(result) if result else 0}.")
    return result

def main():
    SWrite("Script started.")
    try:
        prompt = sys.argv[1] if len(sys.argv) > 1 else ""
        if "~~" not in prompt:
            EWrite("Invalid input format received (missing '~~').")
            print("Invalid input format: expected 'query~~output'")
            return

        query, output = prompt.split("~~", 1)
        query, output = query.strip(), output.strip()
        SWrite(f"Parsed input. Query length: {len(query)}, Output length: {len(output)}.")

        # Step 1: Extract details
        extracted = step1_extract_details(output)

        # Step 2: Explain based on extracted details
        explanation = step2_explain(query, extracted)

        print(explanation or "[Empty response from AI]")
        SWrite("Script finished successfully.")
    except Exception as ex:
        EWrite(f"Unhandled exception: {ex}")
        print(f"[Exception] {ex}")

if __name__ == "__main__":
    main()
