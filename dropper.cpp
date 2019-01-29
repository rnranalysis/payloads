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

int sysinfo()
{
	SYSTEM_INFO sysinfo;
	GetSystemInfo(&sysinfo);
	int numCPU = sysinfo.dwNumberOfProcessors;
	if (numCPU < 2)
	{
		exit();
	}
	MEMORYSTATUSEX mem;
	mem.dwLength = sizeof(mem);
	GlobalMemoryStatusEx(&mem);
	if (mem.ullTotalVirtual < 2147483648)
	{
		exit();
	}
	//std::cout << "\nTotal Virtual Avail: " << mem.ullTotalVirtual / DIV << "kb";
	//std::cout << "\nTotal Physical Avail: " << mem.ullTotalPhys / DIV << "kb";
}
int pschk() 
{
	printf("Process Check\n---------------");
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
		_tprintf(TEXT("\nName:  %s"), s);
		while (Process32Next(ps, &psinfo))
		{
			if (wcsstr(s, L"vmacthlp.exe") || (wcsstr(s, L"vmtoolsd.exe")) || (wcsstr(s, L"ProcessHacker.exe")) || (wcsstr(s, L"ProcessHacker.exe")) || (wcsstr(s, L"Procmon")) || (wcsstr(s, L"Regshot")) || (wcsstr(s, L"ollydbg.exe")) || (wcsstr(s, L"x32dbg")) || (wcsstr(s, L"x64dbg")) || (wcsstr(s, L"ida")) || (wcsstr(s, L"Wireshark.exe")))
			{
				exit();
			}
		}
		CloseHandle(ps);
	}
	return 0;
}

int main()
{
	sysinfo();
	pschk();
}
