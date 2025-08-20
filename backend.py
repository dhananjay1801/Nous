# backend.py
import sys, os, re, requests, groq, yaml
from dotenv import load_dotenv
from Utils.Logger import Logger
from langchain_community.vectorstores import FAISS
from langchain_google_genai import GoogleGenerativeAIEmbeddings, ChatGoogleGenerativeAI

load_dotenv()

# ---------------- Config ---------------- #
GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")
GROQ_API_KEY = os.getenv("GROQ_API_KEY")
LLM_PROVIDER = os.getenv("LLM_PROVIDER", "gemini")  # local | groq | gemini
FAISS_PATH = "vector_db"
EMBED_MODEL = "models/embedding-001"
TOP_K = 3
THRESHOLD = 0.85


# ---------------- Helpers ---------------- #
def get_vector_store():
    if not os.path.exists(FAISS_PATH):
        Logger.EWrite(f"[backend] Vector DB missing at {FAISS_PATH}")
        return None
    try:
        embeddings = GoogleGenerativeAIEmbeddings(model=EMBED_MODEL, google_api_key=GEMINI_API_KEY)
        return FAISS.load_local(FAISS_PATH, embeddings, allow_dangerous_deserialization=True)
    except Exception as e:
        Logger.EWrite(f"[backend] Failed to load FAISS DB: {e}")
        return None


def is_valid_cmd(cmd: str) -> bool:
    if not cmd or "\n" in cmd: return False
    if re.search(r"(request|description|command)\s*:", cmd, re.I): return False
    return True


# ---------------- LLM Calls ---------------- #
def call_gemini(prompt: str):
    try:
        llm = ChatGoogleGenerativeAI(model="gemini-2.0-flash", temperature=0.0, google_api_key=GEMINI_API_KEY)
        return llm.invoke(prompt).content
    except Exception as e:
        Logger.EWrite(f"[backend][gemini] {e}")
        return None

def call_groq(prompt: str):
    try:
        client = groq.Groq(api_key=GROQ_API_KEY)
        resp = client.chat.completions.create(
            messages=[{"role": "user", "content": prompt}],
            model="llama3-8b-8192",
            temperature=0.0
        )
        return resp.choices[0].message.content
    except Exception as e:
        Logger.EWrite(f"[backend][groq] {e}")
        return None

def call_local(prompt: str):
    try:
        payload = {"model": "llama3.2", "messages": [{"role": "user", "content": prompt}], "stream": False}
        resp = requests.post("http://localhost:11434/api/chat", json=payload)
        return resp.json()['message']['content']
    except Exception as e:
        Logger.EWrite(f"[backend][local] {e}")
        return None

def ask_llm(prompt: str):
    if LLM_PROVIDER == "groq": return call_groq(prompt)
    if LLM_PROVIDER == "local": return call_local(prompt)
    return call_gemini(prompt)


# ---------------- Main ---------------- #
def main():
    query = " ".join(sys.argv[1:]).strip()
    if not query:
        print("echo [!] No request provided")
        return

    Logger.SWrite(f"[backend] Prompt: {query}")
    vs = get_vector_store()

    # Step 1: Try RAG
    best_cmd, best_score = None, 0.0
    if vs:
        try:
            results = vs.similarity_search_with_score(query, k=TOP_K)
            for doc, score in results:
                candidate = doc.page_content.strip()
                if is_valid_cmd(candidate) and score > best_score:
                    best_cmd, best_score = candidate, score
                    Logger.SWrite(f"[backend] RAG candidate: {candidate} (score={score:.2f})")
        except Exception as e:
            Logger.EWrite(f"[backend] RAG error: {e}")

    if best_cmd and best_score >= THRESHOLD:
        print(best_cmd)
        return

    # Step 2: Ask LLM fallback
    llm_prompt = f"""
    IMPORTANT: Output ONLY a valid Windows CMD command.
    Do not include quotes, YAML, JSON, or explanations.
    Request: {query}
    """
    raw = ask_llm(llm_prompt)
    if raw:
        cmd = raw.strip().splitlines()[0]
        if is_valid_cmd(cmd):
            print(cmd)
            return

    # Step 3: Last fallback
    if best_cmd:
        Logger.SWrite("[backend] Falling back to weaker RAG candidate")
        print(best_cmd)
    else:
        print("echo [!] Could not generate a valid command")


if __name__ == "__main__":
    main()
