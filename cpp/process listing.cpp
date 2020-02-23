#include "pch.h"
#include <iostream>
#include <Windows.h>
#include <tlhelp32.h>
#include <tchar.h>
#include <cstring>
#include <string.h>

HANDLE ps;
PROCESSENTRY32 psinfo;

int main()
{
	printf("Process Listing\n---------------");
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
		_tprintf( TEXT("\nName:  %s"), s);
		while (Process32Next(ps, &psinfo))
		{
			_tprintf(TEXT("\nName: %s"), s);
		}
		CloseHandle(ps);
	}
}
