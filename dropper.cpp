#include "pch.h"
#include <iostream>
#include <Windows.h>
#include <tlhelp32.h>
#include <tchar.h>
#include <cstring>
#include <string.h>
#include <strsafe.h>
#pragma comment(lib, "User32.lib")
#define DIV 1024

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
		WCHAR* s = psinfo.szExeFile;
		while (Process32Next(ps, &psinfo))
		{
			if (wcsstr(s, L"vmacthlp.exe") || (wcsstr(s, L"vmtoolsd.exe")) || (wcsstr(s, L"ProcessHacker.exe")) || (wcsstr(s, L"ProcessHacker.exe")) || (wcsstr(s, L"Procmon")) || (wcsstr(s, L"Regshot")) || (wcsstr(s, L"ollydbg.exe")) || (wcsstr(s, L"x32dbg")) || (wcsstr(s, L"x64dbg")) || (wcsstr(s, L"ida")) || (wcsstr(s, L"Wireshark.exe")))
			{
				return 1;
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
		std::cout << "FindFirstFile: INVALID HANDLE" << std::endl;
	}

	while (FindNextFileW(hFind, &w32fd))
	{
		WCHAR* s = w32fd.cFileName;
		if (wcsstr(s, L"Wireshark") || wcsstr(s, L"Process Hacker"))
		{
			//std::wcout << "Filename: " << w32fd.cFileName << std::endl;
			return 1;
		}
		
	}
	return 0;

	if (ERROR_NO_MORE_FILES != GetLastError())
 {
		std::cout << "NO MORE FILES IN DIR" << std::endl;
 }
	FindClose(hFind);
	return 0;
}

int dirchk()
{
	LPSTR cmdline = GetCommandLineA();
	if (strstr(cmdline, "\\repos\\dropper\\Debug"))
	{
		std::cout << "Correct Directory!" << std::endl;
	}
	else
	{
		std::cout << "Incorrect Directory!" << cmdline << std::endl;
	}
	std::cout << "Commandline: " << cmdline << std::endl;
	return 0;
}
int main()
{
	dirchk();
	if (dbgchk()) // return 1 if dbgflag is set, 0 if not
	{
		std::cout << "Exit: Debugger detected!" << std::endl;
		//exit();
	}
	if (appschk())
	{
		std::cout << "Exit: Analyst tool installed!" << std::endl;
		//exit();
	}
	if (sysinfo() == 2) // return 2 if low processor count, return 1 low ram 
	{
		std::cout << "Exit: Low process count" << std::endl;
		//exit();
	}
	else if (sysinfo() == 1)
	{
		//std::cout << "Exit: Low RAM" << std::endl;
	}
	if (pschk()) // return 1 if analyst tools is in process listing
	{
		std::cout << "Exit: Analyst tool running!" << std::endl;
		//exit();
	}
}

