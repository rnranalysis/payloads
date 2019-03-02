import socket
import sys
import os

cmd = ""


def create():
    try:
        global host
        global port
        global s
        host = 'localhost'
        port = 9999
        s = socket.socket()
    except socket.error as msg:
        print("Error: " + str(msg))


def bind():
    try:
        print("Listening on port  " + str(port))
        s.bind((host, port))
        s.listen(5)
    except socket.error as msg:
        print("Error: " + str(msg))
        socket_bind()


def accept():
    conn, address = s.accept()
    print("Client connection established: " + "IP " + address[0] + "  Port: " + str(address[1]))
    ssend(conn)
    conn.close()


def ssend(conn):
    while True:
        cmd = raw_input("demo$ ")
        args = cmd.split()
        if len(cmd) < 1:
            continue
        elif cmd == 'quit' or cmd == 'exit':
            conn.send('goodbye!')
            conn.close()
            s.close()
            sys.exit()
        elif args[0] == 'run':
            conn.send('run ' + str(os.path.getsize(args[1])))
            resp = conn.recv(1024)
            if resp[:5].decode("utf-8") == 'ready':
                f = open(args[1],'r')
                data = f.read(1024)
                while (data):
                    conn.send(data)
                    data = f.read(1024)
                f.close()
                print('PAYLOAD SENT.')
                data = ''
                while True:
                    client_response = str(conn.recv(1024))
                    data += client_response
                    if len(client_response) < 1024:
                        break
                print(data)
        else:
             print('this is a demo.....try <demo$ run someScript.py>')


def main():
    create()
    bind()
    accept()

main()
