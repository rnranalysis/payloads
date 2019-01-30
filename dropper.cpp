#include "pch.h"
#include <iostream>
#include <Windows.h>
#include <tlhelp32.h>
#include <tchar.h>
#include <cstring>
#include <string.h>

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
		return 1;
	}
	MEMORYSTATUSEX mem;
	mem.dwLength = sizeof(mem);
	GlobalMemoryStatusEx(&mem);
	if (mem.ullTotalVirtual < 2147483648)
	{
		return 1;
	}
	else
	{
		return 0;
	}
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

void read_directory()
{
	LPCWSTR dir = L"C:\\Program Files\\";
	WIN32_FIND_DATA data;
	HANDLE hFile;
	if (!FindFirstFile(dir, &data))
	{
		int err = GetLastError();
		std::cout << "FindFirstFile Error: " << err;
	} 
	while (FindNextFile(hFile, &data))
	{

		FindClose(hFile);
	}
}
int main()
{
	if (dbgchk()) // return 1 if dbgflag is set, 0 if not
	{
		std::cout << "Debugger detected!";
		exit();
	}
	if (sysinfo()) // return 1 if low processor count or ram 
	{
		std::cout << "Low process count/ram";
	}
	if (pschk()) // return 1 if analyst tools is in process listing
	{
		std::cout << "Analyst tool(s) detected!";
		//exit();
	}
}
