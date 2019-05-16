#include "pch.h"
#include <iostream>
#include <Windows.h>
#include <WinUser.h>

const char * GetClip()
{
	if (OpenClipboard(NULL))
	{
		HANDLE hData = GetClipboardData(CF_TEXT);
		char * buffer = (char*)GlobalLock(hData);
		//std::cout << buffer << std::endl;
		printf("\n%s", buffer);
		GlobalUnlock(hData);
		CloseClipboard();
		return buffer;
	}
	else
	{
		return "no data!";
	}
}


void getwindowTitle(char * outStr)
{
	HWND hWind = GetForegroundWindow();
	if (hWind == NULL)
	{
		printf("GetForegroundWindow Error!\n");
	}
	else
	{
		
		int s = GetWindowTextA(hWind, outStr, 256);
		printf("Window Title: %s\n", outStr);
	}
}



int main()
{
	while (1)
	{
		char* outStr = new char[256];		
		getwindowTitle(outStr);
		if (strstr(outStr, "Internet Explorer") != NULL)
		{
			printf("IE found -> %s\n", outStr);
			delete outStr;
			break;
			
		}
		else
		{
			printf("%s is not iexplore!\n", outStr);
			delete outStr;
			Sleep(5000);
			continue;
		}
	}
	while (1)
	{
		GetClip();
		Sleep(5000);
	}
}

