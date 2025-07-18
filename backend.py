import sys
import os
import requests
import ollama
# from dotenv import load_dotenv
from bs4 import BeautifulSoup
from IPython.display import Markdown, display
# from openai import OpenAI



def main():
    prompt = sys.argv[1] if len(sys.argv) > 1 else ""
    OLLAMA_API = "http://localhost:11434/api/chat"
    HEADERS = {"Content-Type": "application/json"}
    MODEL = "llama3.2"
    # print("whoami")
    messages = [
    {"role": "user", "content": " Dont write anything other than the single command in output. The command is to be ran in CMD do not add anywhere in the code, write the command only to know"+prompt+"Do not add any single apstrophes or anything else JUST THE RAW COMMAND which I can copy paste into terminal"}
        ]
    payload = {
        "model": MODEL,
        "messages": messages,
        "stream": False
    }
    response = requests.post(OLLAMA_API, json=payload, headers=HEADERS)
    print(response.json()['message']['content'])

if __name__ == "__main__":
    main()
