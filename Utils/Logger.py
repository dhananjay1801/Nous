import os
from pathlib import Path
from dotenv import load_dotenv
from datetime import datetime

load_dotenv()
BASE_PATH = Path(os.getenv("BASE_PATH", Path(__file__).resolve().parents[1]))

class Logger:
    # Same path as your C# logger (built from BASE_PATH)
    Path = str(BASE_PATH / "Log.txt")

    @staticmethod
    def SWrite(message: str):
        """Success/info log: [+] [dd/MM/yyyy HH:mm:ss] message"""
        try:
            # Ensure directory & file exist
            directory = os.path.dirname(Logger.Path)
            if directory and not os.path.exists(directory):
                os.makedirs(directory, exist_ok=True)
            if not os.path.exists(Logger.Path):
                open(Logger.Path, "a", encoding="utf-8").close()

            # en-GB style: dd/MM/yyyy HH:mm:ss
            stamp = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
            with open(Logger.Path, "a", encoding="utf-8") as f:
                f.write(f"[+] [{stamp}] {message}\n")
        except Exception as ex:
            # Don't raise from logger; mirror your C# behavior (just swallow/log to console)
            try:
                print(f"Logger Error: {ex}")
            except Exception:
                pass

    @staticmethod
    def EWrite(message: str):
        """Error log: [-] [dd/MM/yyyy HH:mm:ss] message"""
        try:
            directory = os.path.dirname(Logger.Path)
            if directory and not os.path.exists(directory):
                os.makedirs(directory, exist_ok=True)
            if not os.path.exists(Logger.Path):
                open(Logger.Path, "a", encoding="utf-8").close()

            stamp = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
            with open(Logger.Path, "a", encoding="utf-8") as f:
                f.write(f"[-] [{stamp}] {message}\n")
        except Exception as ex:
            try:
                print(f"Logger Error: {ex}")
            except Exception:
                pass
