# ⚡ Nous – AI-Powered Secure Remote Administration  

![Nous Banner](Ouroboros.png)  

---

[![Made with Python](https://img.shields.io/badge/Python-3.11-blue?logo=python)](https://www.python.org/)  
[![Made with C#](https://img.shields.io/badge/C%23-AvaloniaUI-purple?logo=csharp)](https://avaloniaui.net/)  
[![AI Powered](https://img.shields.io/badge/AI-Google_Gemini_2.5_Flash-orange?logo=google)](https://deepmind.google/)  
[![Database](https://img.shields.io/badge/Database-SQL+VectorDB-green?logo=sqlite)]()  
[![License](https://img.shields.io/badge/License-MIT-black.svg)](LICENSE)  

---

## 🚀 What is Nous?  
Nous is a **secure, AI-assisted system administration tool** that empowers admins to control remote systems using **natural language**.  
It combines **AI command generation**, **TLS encryption**, and **hash-based obscurity** to ensure safe and intelligent remote execution.  

👉 Instead of remembering commands, just **type what you want** – Nous handles the rest.  

---

## ✨ Key Features  

✅ **Secure by Design**  
- TLS-encrypted socket communication (RSA keys)  
- Hourly hash regeneration for listener identity  

✅ **AI-Assisted Commands**  
- Natural language → System command via **Gemini 2.5 Flash**  
- **RAG + Vector DB** ensures reliable command mapping  

✅ **Cross-Platform Admin GUI**  
- Built with **Avalonia UI** (C#)  
- Sleek interface to manage listeners & execute commands  

✅ **Smart Feedback**  
- AI reformats raw execution outputs into **human-readable results**  
- Suggests **fixes** when commands fail  

✅ **Scalable & Fast**  
- Multithreaded output processing  
- SQL for IP + hash storage  

---

## 🛠️ Tech Stack  

- **Languages:** Python, C#  
- **Frameworks:** Avalonia UI, TLS (RSA), Multithreading  
- **AI Models:** Google Gemini 2.5 Flash (via API)  
- **Databases:** SQL + Vector DB (for RAG)  
- **Security:** Hash-based identity, RSA keypair, TLS sockets  

---
## 🧑‍🔧 STEPS TO INSTALL NOUS ON YOUR PC
1. git clone the GitHub link of my project
2. Open the project in visual studio
3. Install dependencies --> Requirements.txt
4. Run create_vector_db.py to create the vector database --> Only need to run it once
5. Install MySQL and make the database, enter its details in IP. Update its username, database name, password and host in 1.api_server.py file and 2. IP hash manager.cs,update the paths to cert.pem/key.pem/log.txt wherever mentioned. 
6. Build and Open Nous. 
7. Start the server by clicking on start server. This should open a HTTP server on your host side
8. ON THE CLIENT SIDE - Unzip Listener-v.2 (for running safe version of listener) OR Unzip Hyper-Listener-v.2 (for running administrator version of listener). 
9. ON THE CLIENT SIDE - Update the IP address in listener file to that of the host and run the listener, also update the path to the cert.pem and key.pem . This should register your IP address on the SQL server on the host side, copy the hashcode viewed there.
10. Now you're ready to use the Access Gate, Enter the hashcode from the listener with the prompt. 
11. Click generate and the command will be generated, Ensure the command is safe and correct after generation and then you can send the command to the listeners.
12. For any kind of errors or problems in execution, refer the LOG file, else contact me @swamiomkar262004@gmail.com | @omkar-swami-619788259 on linkedin| @0mkxrs on Instagram
---


## ⚙️ How It Works  

![Nous Architecture](Assets/MAINPROCESSSFLOW.jpg)  

*Main Process Workflow*
1. **Host API server** starts.  
2. **Listener** registers → DB stores hash instead of raw IP.  
3. **Admin enters hash + natural language prompt**.  
4. **AI generates system command** → sent securely via TLS.  
5. **Listener executes command** → sends result back.  
6. **AI reformats output** → GUI shows success, output, or suggested fix.  
7. **Every hour, hash regenerates**.  

---

## 📸 Demo  

### 🏠 Landing Page  
![Landing Page](Assets/LandingPageUIofNous.jpg)  
*️The homepage and initial UI of Nous.*  

### ⚡ Start Server Dialogue  
![Start Server](Assets/StartServerDialogueBox.jpg)  
*️Appears when you start the server (mandatory before starting listeners).*  

### 🔑 Access Gate  
![Access Gate](Assets/AccessGate.jpg)  
*️Main functionality page – enter hashcodes, generate/edit/send commands.*  


---

### 📂 Example Results  

- **Basic Execution (Flask install)**  
![Result Flask](Assets/resultinstallflask.jpg)  
*️Example: Running a command to install Flask.*  

- **Formatted Output**  
![Result Format](Assets/resultformat.jpg)  
*️Example: Cannot do malicious tasks with this project, proper sanitization, this image shows that anyone cannot run any commands which might harm the listener.*  

- **Internet Check**  
![Result Internet](Assets/resultintenet.jpg)  
*️Example: Checking internet connectivity.*  

- **DNS Resolution**  
![Result DNS](Assets/resultdns.jpg)  
*️Example: Testing DNS-related command.*  

- **Python Environment**  
![Result Python](Assets/resultpython.jpg)  
*️Example: Checking Python version/configuration.*  

- **Java Environment**  
![Result Java](Assets/resultjava.jpg)  
*️Example: Checking Java installation.*  

- **System Shutdown Command**  
![Result Shutdown](Assets/resultshutdown.jpg)  
*️Example: Attempting a shutdown command with AI interpretation which is stopped.*  

- **Notepad Command**  
![Result Notepad](Assets/resultnotepad.jpg)  
*️Example: Checking if notepad is open in background.*  

---

### ℹ️ About Section  
![About Nous](Assets/about.jpg)  
*️About page of the application – project details & credits.*  

---

## 🎯 Use Cases  
- Secure remote **system administration**  
- AI-powered **DevOps assistant**  
- Natural language automation for **IT support**  
- **Error analysis** + guided fixes  

---

## 🧑‍💻 Team  

👑 **Project Lead:** Omkar Swami – Architecture, AI Integration, Security, Orchestration  
🤝 **Team Members:** Dhananjay Tailor, Rushbh Mistry, Prashant Chauhan

---

## 🚀 Future Roadmap  

- 🔐 Role-based access control (RBAC)  
- 🌐 Web dashboard with real-time monitoring  
- 📊 Logs + Analytics for executed commands  
- 🤖 Multi-model AI integration  

---

## 📄 License  

This project is licensed under the **MIT License** – see the [LICENSE](LICENSE) file for details.  


---

## ⭐ Support  

If you like this project, give it a **star ⭐** on GitHub – it helps a lot!

## ❣️Thanks for scrolling through. 😁 HAVE A NICE DAY!
