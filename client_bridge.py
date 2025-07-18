import socket
import ssl
import sys

def main():
    if len(sys.argv) != 3:
        print("Usage: python client_bridge.py <server_ip> <command>")
        return

    server_ip = sys.argv[1]
    command = sys.argv[2]
    server_port = 8443  # Must match your listener.py

    # Path to the server's certificate (for verifying identity)
    cert_file = 'cert.pem'  # This should be the same cert used in listener.py

    context = ssl.create_default_context(ssl.Purpose.SERVER_AUTH, cafile=cert_file)

    try:
        with socket.create_connection((server_ip, server_port)) as sock:
            with context.wrap_socket(sock, server_hostname="localhost") as ssock:
                ssock.sendall(command.encode('utf-8'))
                response = ssock.recv(8192).decode('utf-8')
                print("Response from server:\n", response)
    except Exception as e:
        print("Error:", e)

if __name__ == "__main__":
    main()
