using System;
using System.Windows.Forms;

namespace Social_Savior {
    static class Program {
        public static Form MainFormInstance => Application.OpenForms[0];
        public static bool Execute = false;
        public static bool Startup = false;
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main(string[] Args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Args?.Length > 0 && Args[0].Trim(' ', '/', '-', '\\').ToLower().StartsWith("startup"))
                Startup = true;

            if (!Startup) {
                Application.Run(new Update());
                if (!System.Diagnostics.Debugger.IsAttached)
                    Application.Run(new Welcome());
            }

            if (Execute || Startup || System.Diagnostics.Debugger.IsAttached)
                Application.Run(new Main());

            Environment.Exit(0);
        }
    }
}
