using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Ionic.Zip;

class AppVeyor {
    const string Info = "build";

    string API = "https://ci.appveyor.com/api/projects/{0}/{1}/";
    string Artifact = string.Empty;
    public static string MainExecutable = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
    public static string TempUpdateDir = Path.GetDirectoryName(MainExecutable) + "\\AppVeyorUpdate\\";
    public static string CurrentVersion {
        get {
            var Version = FileVersionInfo.GetVersionInfo(MainExecutable);
            return Version.FileMajorPart + "." + Version.FileMinorPart;
        }
    }
    public AppVeyor(string Username, string Project, string Artifact) {
        API = string.Format(API, Username, Project);
        this.Artifact = Artifact;

        if (!File.Exists(MainExecutable))
            throw new Exception("Failed to Catch the Executable Path");
    }

    public string FinishUpdate() {
        if (FinishUpdatePending()) {
            int Len = MainExecutable.IndexOf("\\AppVeyorUpdate\\");
            string OriginalPath = MainExecutable.Substring(0, Len);
            foreach (string File in Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories)) {
                string Base = File.Substring(Environment.CurrentDirectory.Length + 1);
                string UpPath = Environment.CurrentDirectory + "\\" + Base;
                string OlPath = OriginalPath + "\\" + Base;

                Delete(OlPath);
                System.IO.File.Copy(UpPath, OlPath);
            }

            return OriginalPath + "\\" + Path.GetFileName(MainExecutable);
        } else {
            new Thread(() => {
                while (Directory.Exists(TempUpdateDir)) {
                    try {
                        Directory.Delete(TempUpdateDir, true);
                    } catch { Thread.Sleep(1000); }
                }
            }).Start();

            return null;
        }
    }

    private void Delete(string File) {
        for (int Tries = 0; Tries < 10; Tries++) {
            string ProcName = Path.GetFileNameWithoutExtension(File);
            Process[] Procs = Process.GetProcessesByName(ProcName);
            int ID = Process.GetCurrentProcess().Id;
            foreach (var Proc in Procs) {
                if (Proc.Id == ID)
                    continue;
                try {
                    Proc.Kill();
                    Thread.Sleep(100);
                } catch { }
            }
            try {
                if (System.IO.File.Exists(File))
                    System.IO.File.Delete(File);
            } catch {
                Thread.Sleep(100);
                continue;
            }
            break;
        }
    }
    public bool HaveUpdate() {
        try {
            if (Debugger.IsAttached)
                return false;

            string CurrentVersion = FileVersionInfo.GetVersionInfo(MainExecutable).FileVersion.Trim();
            string LastestVersion = GetLastestVersion().Trim();
            int[] CurrArr = CurrentVersion.Split('.').Select(x => int.Parse(x)).ToArray();
            int[] LastArr = LastestVersion.Split('.').Select(x => int.Parse(x)).ToArray();
            int Max = CurrArr.Length < LastArr.Length ? CurrArr.Length : LastArr.Length;
            for (int i = 0; i < Max; i++) {
                if (LastArr[i] > CurrArr[i])
                    return true;
                if (LastArr[i] == CurrArr[i])
                    continue;
                return false;//Lst<Curr
            }
            return false;
        } catch { return false; }
    }

    public bool FinishUpdatePending() {
        if (MainExecutable.Contains("\\AppVeyorUpdate\\")) 
            return true;

        return false;
    }
    public void Update() {
        if (!HaveUpdate())
            return;

        string Result = FinishUpdate();
        if (Result != null) {
            Process.Start(Result);
            Environment.Exit(0);
        }

        MemoryStream Update = new MemoryStream(Download(API + "artifacts/" +  Artifact.Replace(" ", "%20").Replace("\\", "/")));
        var Zip = ZipFile.Read(Update);
        if (Directory.Exists(TempUpdateDir))
            Directory.Delete(TempUpdateDir, true);

        Directory.CreateDirectory(TempUpdateDir);
        Zip.ExtractAll(TempUpdateDir, ExtractExistingFileAction.OverwriteSilently);
        Process.Start(TempUpdateDir + Path.GetFileName(MainExecutable));
        Environment.Exit(0);
    }

    private void Backup(string Path) {
        if (File.Exists(Path + ".bak"))
            File.Delete(Path + ".bak");
        while (File.Exists(Path)) {
            try {
                File.Move(Path, Path + ".bak");
            } catch { Thread.Sleep(100); }
        }
    }

    private byte[] Download(string URL) {
        MemoryStream MEM = new MemoryStream();
        Download(URL, MEM);
        byte[] DATA = MEM.ToArray();
        MEM.Close();
        return DATA;
    }

    private void Download(string URL, Stream Output, int tries = 4) {
        try {
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(URL);
            //Bypass a fucking bug in the fucking .net framework
            if (Request.Address.AbsoluteUri != URL && tries <= 2) {
                /*
                WebClient WC = new WebClient();
                WC.QueryString.Add("action", "shorturl");
                WC.QueryString.Add("format", "simple");
                WC.QueryString.Add("url", URL);
                URL = WC.DownloadString("https://u.nu/api.php");*/

                Request = (HttpWebRequest)WebRequest.Create("http://proxy-it.nordvpn.com/browse.php?u=" + URL);
                Request.Referer = "http://proxy-it.nordvpn.com";
            }

            Request.UseDefaultCredentials = true;
            Request.Method = "GET";
            WebResponse Response = Request.GetResponse();
            byte[] FC = new byte[0];
            using (Stream Reader = Response.GetResponseStream()) {
                byte[] Buffer = new byte[1024];
                int bytesRead;
                do {
                    bytesRead = Reader.Read(Buffer, 0, Buffer.Length);
                    Output.Write(Buffer, 0, bytesRead);
                } while (bytesRead > 0);
            }
        } catch (Exception ex) {
            if (tries < 0)
                throw new Exception(string.Format("Connection Error: {0}", ex.Message));

            Thread.Sleep(1000);
            Download(URL, Output, tries - 1);
        }
    }

    private string GetLastestVersion() {
        string Reg = "\"version\":[\\s]*\"([0-9.]*)\"";
        string XML = new WebClient().DownloadString(API);
        var a = System.Text.RegularExpressions.Regex.Match(XML, Reg);
        return a.Groups[1].Value;
    }

}