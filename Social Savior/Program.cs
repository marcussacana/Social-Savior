using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace Social_Savior {
    static class Program {

        static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static Form MainFormInstance => Application.OpenForms[0];
        public static bool Execute = false;
        public static bool Startup = false;
        public static bool Restart = false;
        public static bool Updates = true;
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main(string[] Args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Args?.Length > 0) {
                string CMD = Args[0].Trim(' ', '/', '-', '\\', ';').ToLower();
                if (CMD.Contains("startup"))
                    Startup = true;

                if (CMD.Contains("restart"))
                    Restart = true;

                if (CMD.Contains("panic") && SingleInstanceService.PipeIsOpen())
                {
                    SingleInstanceService.RequestPanic();
                    return;
                }

                if (CMD.Contains("restore") && SingleInstanceService.PipeIsOpen())
                {
                    SingleInstanceService.RequestRestore();
                    return;
                }

                if (CMD.Contains("noupdate") || CMD.Contains("noup"))
                    Updates = false;
            }

            if (!IsElevated && !Debugger.IsAttached && !Restart) {
                Process.Start(new ProcessStartInfo() {
                    Arguments = Startup ? "startup;restart" : "restart",
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                });
                Process.GetCurrentProcess().Kill();
            }

            if (Restart)
                Thread.Sleep(1000);

            if (Startup && !File.Exists(Social_Savior.Main.SettingsPath))
                return;
            

            if (!Startup) {
                if (Updates)
                    Application.Run(new Update());
                if (!Debugger.IsAttached)
                    Application.Run(new Welcome());
            }

            if (Execute || Startup || Debugger.IsAttached)
                Application.Run(new Main());

            Environment.Exit(0);
        }

        #region Non-Windows Support

        [DllImport(@"kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        static bool? isRWin;

        internal static bool IsRealWindows {
            get {
                if (isRWin.HasValue)
                    return isRWin.Value;

                IntPtr hModule = GetModuleHandle(@"ntdll.dll");
                if (hModule == IntPtr.Zero)
                    isRWin = false;
                else
                {
                    IntPtr fptr = GetProcAddress(hModule, @"wine_get_version");
                    isRWin = fptr == IntPtr.Zero;
                }

                return isRWin.Value;
            }
        }
        #endregion

        public static void Log(string Text)
        {
            Console.WriteLine(Text);
            if (Text.StartsWith("EVENT: "))
            {
                return;
            }

            using (StreamWriter Writer = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "Social Savior.log")) {
                Writer.WriteLine($"{DateTime.Now.ToShortTimeString()}: {Text}");
                Writer.Close();
            }
        }
    }
}
