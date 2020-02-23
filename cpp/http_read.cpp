#include "pch.h"
#include <iostream>
#include <Windows.h>
#include <wininet.h>
#include <string>
using namespace std;

int main()
{
	static const char *acceptTypes[] = { "application/x-www-form-urlencoded", NULL };
	HINTERNET hInternet = InternetOpenA("rnrhttp", INTERNET_OPEN_TYPE_DIRECT, NULL, NULL, 0);
	if (hInternet == 0)
	{
		cout << "InternetOpen Error: " << GetLastError() << endl;
	}
	else
	{
		HANDLE conn = InternetConnectA(hInternet, "www.google.com", INTERNET_DEFAULT_HTTP_PORT, NULL, NULL, INTERNET_SERVICE_HTTP, 0, 0);
		if (conn == 0)
		{
			cout << "InternetConnect Error: " << GetLastError() << endl;
		}
		else
		{
			HINTERNET hOpenRequest = HttpOpenRequestA(conn, "GET", "/index.html", "HTTP/1.1", NULL, acceptTypes, INTERNET_FLAG_RELOAD, NULL);
			if (hOpenRequest == 0)
			{
				cout << "HttpOpenRequest Error: " << GetLastError << endl;
			}
			else
			{
				if (!HttpSendRequestA(hOpenRequest, NULL, -1, NULL, 0))
				{
					cout << "HttpSendRequest Error: " << GetLastError() << endl;
				}
				else
				{
					DWORD blocksize = 4096;
					DWORD received = 0;
					string temp;
					string block(blocksize, 0);
					while (InternetReadFile(hOpenRequest, &block[0], blocksize, &received) && received)
					{
						block.resize(received);
						temp += block;
					}
					cout << "Data: " << temp << endl;
				}
			}
		}
	}
	return 0;
}
