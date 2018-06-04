using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Social_Savior {
    public partial class Update : Form {
        public Update() {
            InitializeComponent();
            new Task(() => {
                try {
                    GitHub Updater = new GitHub("Marcussacana", "Social-Savior");

                    if (Updater.FinishUpdatePending()) {
                        SingleInstanceService.RequestQuit();
                    }

                    string Result = Updater.FinishUpdate();
                    if (Result != null) {
                        Process.Start(Result);
                        Environment.Exit(0);
                    }

                    if (Updater.HaveUpdate()) {
                        Invoke(new MethodInvoker(() => { lblStatus.Text = "Updating..."; }));
                        Updater.Update();
                    }

                    Invoke(new MethodInvoker(Close));
                } catch (Exception ex) {
                    MessageBox.Show("Auto Updater Exception: \n" + ex.Message + "\n\n" + ex.StackTrace);

                    Invoke(new MethodInvoker(Close));
                }
            }).Start();
        }
    }
}