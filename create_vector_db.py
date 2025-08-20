#!/usr/bin/env python3
"""
create_vector_db.py

Builds a FAISS vector DB from YAML RAG files located under DATA_DIR.
This script *parses* YAML files (not as raw text), extracts only
(request, command) pairs, embeds them, and saves a clean vector index.

Logging is done through Utils.Logger to match your project's logger.
"""
import os
import yaml
from dotenv import load_dotenv
from langchain_community.vectorstores import FAISS
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from langchain.schema import Document
from Utils.Logger import Logger

load_dotenv()

DATA_DIR = "data"
FAISS_PATH = "vector_db"
EMBEDDING_MODEL = "models/embedding-001"  # keeps consistent with backend.py env usage


def load_yaml_entries(filepath):
    """
    Loads YAML from a file that contains either:
      - a top-level list of mappings (common case), or
      - a single mapping (less common)
    Returns a list of dict entries (possibly empty).
    """
    try:
        with open(filepath, "r", encoding="utf-8") as f:
            data = yaml.safe_load(f)
            if data is None:
                return []
            if isinstance(data, list):
                return data
            if isinstance(data, dict):
                # Single document represented as a mapping
                return [data]
            # Other types (string/number) -> ignore
            return []
    except Exception as e:
        Logger.EWrite(f"[create_vector_db] Failed to parse YAML '{filepath}': {e}")
        return []


def create_vector_db(data_dir=DATA_DIR, out_dir=FAISS_PATH):
    Logger.SWrite(f"[create_vector_db] Starting. Scanning '{data_dir}' for .yml files...")
    docs = []

    for root, _, files in os.walk(data_dir):
        for fname in files:
            if not fname.lower().endswith((".yml", ".yaml")):
                continue
            path = os.path.join(root, fname)
            Logger.SWrite(f"[create_vector_db] Loading '{path}'")
            entries = load_yaml_entries(path)
            for entry in entries:
                # Expect each entry to be a mapping with at least 'request' and 'command'
                if not isinstance(entry, dict):
                    continue
                request = entry.get("request") or entry.get("Request")
                command = entry.get("command") or entry.get("Command")
                # skip incomplete entries
                if not request or not command:
                    continue
                # Build a small, clean text for embedding (request + command only)
                content = f"Request: {str(request).strip()}\nCommand: {str(command).strip()}"
                # Add metadata to help debugging / future filtering
                meta = {"source_file": path}
                docs.append(Document(page_content=content, metadata=meta))

    if not docs:
        Logger.EWrite("[create_vector_db] No valid request/command pairs found. Aborting.")
        return

    Logger.SWrite(f"[create_vector_db] Preparing embeddings for {len(docs)} entries...")
    api_key = os.getenv("GEMINI_API_KEY")
    if not api_key:
        Logger.EWrite("[create_vector_db] GEMINI_API_KEY not found in environment. Aborting.")
        return

    try:
        embeddings = GoogleGenerativeAIEmbeddings(model=EMBEDDING_MODEL, google_api_key=api_key)
        Logger.SWrite("[create_vector_db] Creating FAISS vector store (this will take a moment)...")
        vector_store = FAISS.from_documents(docs, embeddings)
        vector_store.save_local(out_dir)
        Logger.SWrite(f"[create_vector_db] Vector database created and saved to '{out_dir}'.")
    except Exception as e:
        Logger.EWrite(f"[create_vector_db] Error while building/saving FAISS DB: {e}")


if __name__ == "__main__":
    Logger.SWrite("\n\n<--- CREATE VECTOR DB START --->\n")
    create_vector_db()
    Logger.SWrite("<--- CREATE VECTOR DB END --->\n\n")
