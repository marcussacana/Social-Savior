using System;
using System.Windows.Forms;

namespace Social_Savior {
    static class Program {
        public static Form MainFormInstance => Application.OpenForms[0];
        public static bool Execute = false;
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Update());

            if (!System.Diagnostics.Debugger.IsAttached)
                Application.Run(new Welcome());

            if (Execute || System.Diagnostics.Debugger.IsAttached)
                Application.Run(new Main());

            Environment.Exit(0);
        }
    }
}
