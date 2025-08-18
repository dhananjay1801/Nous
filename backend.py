import sys
import os
import requests
import groq
from dotenv import load_dotenv
from langchain_community.vectorstores import FAISS
from langchain_google_genai import GoogleGenerativeAIEmbeddings, ChatGoogleGenerativeAI
from Utils.Logger import Logger

load_dotenv()

#GROQ_API_KEY = os.getenv("GROQ_API_KEY")
GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")

LLM_PROVIDER = "gemini" # Options: "local", "groq", "gemini"
FAISS_PATH = "vector_db"


def get_rag_retriever():
    """Loads the pre-existing FAISS vector database from disk."""

    if not os.path.exists(FAISS_PATH):
        Logger.EWrite(f"Vector database not found at {FAISS_PATH}. Please run create_vector_db.py first on the server machine.")
        return None

    Logger.SWrite("Loading FAISS vector database...")
    embeddings = GoogleGenerativeAIEmbeddings(model="models/embedding-001", google_api_key=GEMINI_API_KEY)
    
    try:
        # Load the vector store from the local folder
        vector_store = FAISS.load_local(
            FAISS_PATH, 
            embeddings, 
            allow_dangerous_deserialization=True # Required for loading local FAISS indexes
        )   
        Logger.SWrite("FAISS vector database loaded successfully.")
        return vector_store.as_retriever(search_kwargs={"k": 3})
    except Exception as e:
        Logger.EWrite(f"Error loading FAISS database: {e}")
        return None


def get_llm_response(prompt_content):
    Logger.SWrite(f"Fetching LLM response using provider: {LLM_PROVIDER}")
    if LLM_PROVIDER == "local":
        return call_local_ollama(prompt_content)
    elif LLM_PROVIDER == "groq":
        return call_groq_api(prompt_content)
    elif LLM_PROVIDER == "gemini":
        return call_gemini_api(prompt_content)
    else:
        Logger.EWrite(f"Invalid LLM_PROVIDER: {LLM_PROVIDER}")
        raise ValueError(f"Invalid LLM_PROVIDER: {LLM_PROVIDER}")


def call_local_ollama(prompt_content):
    payload = {"model": "llama3.2", "messages": [{"role": "user", "content": prompt_content}], "stream": False, "options": {"temperature": 0.0}}
    try:
        response = requests.post("http://localhost:11434/api/chat", json=payload)
        response.raise_for_status()
        Logger.SWrite("Local Ollama model responded successfully.")
        return response.json()['message']['content']
    except requests.exceptions.RequestException as e:
        Logger.EWrite(f"Error contacting local Ollama model: {e}")
        return f"Error contacting local Ollama model: {e}"


def call_groq_api(prompt_content):
    try:
        client = groq.Groq(api_key=GROQ_API_KEY)
        chat_completion = client.chat.completions.create(
            messages=[{"role": "user", "content": prompt_content}], 
            model="llama3-8b-8192", 
            temperature=0.0
        )
        Logger.SWrite("Groq API responded successfully.")
        return chat_completion.choices[0].message.content
    except Exception as e:
        Logger.EWrite(f"Error contacting Groq API: {e}")
        return f"Error contacting Groq API: {e}"


def call_gemini_api(prompt_content):
    try:
        llm = ChatGoogleGenerativeAI(model="gemini-2.0-flash", temperature=0.0, google_api_key=GEMINI_API_KEY)
        response = llm.invoke(prompt_content)
        Logger.SWrite("Gemini API responded successfully.")
        return response.content
    except Exception as e:
        Logger.EWrite(f"Error contacting Gemini API: {e}")
        return f"Error contacting Gemini API: {e}"


def main():
    user_prompt = sys.argv[1] if len(sys.argv) > 1 else ""
    Logger.SWrite(f"Received user prompt: {user_prompt}")

    retriever = get_rag_retriever()
    
    retrieved_context = ""
    if retriever:
        try:
            retrieved_docs = retriever.invoke(user_prompt)
            retrieved_context = "\n\n---\n\n".join([doc.page_content for doc in retrieved_docs])
            Logger.SWrite("Successfully retrieved documents from FAISS retriever.")
        except Exception as e:
            Logger.EWrite(f"Error retrieving documents: {e}")

    final_prompt = f"""
        Based on the following examples if available:
        ---
        {retrieved_context}
        ---
        Generate ONLY the raw Windows CMD command for the following request. Do not add any explanation, code blocks, or any text other than the command itself.

        Request: "{user_prompt}"
        """
    generated_command = get_llm_response(final_prompt)

    Logger.SWrite(f"Generated command: {generated_command.strip()}")
    print(generated_command.strip())


if __name__ == "__main__":
    Logger.SWrite("Starting script execution...")
    main()
