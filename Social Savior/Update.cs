using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Social_Savior {
    public partial class Update : Form {
        public Update() {
            InitializeComponent();
            new Task(() => {
                AppVeyor Updater = new AppVeyor("Marcussacana", "Social-Savior", "Social Savior\\bin\\Social Savior.zip");
                string Result = Updater.FinishUpdate();
                if (Result != null) {
                    System.Diagnostics.Process.Start(Result);
                    Environment.Exit(0);
                }

                if (Updater.HaveUpdate()) {
                    Invoke(new MethodInvoker(() => { lblStatus.Text = "Updating..."; }));
                    Updater.Update();
                }

                Invoke(new MethodInvoker(Close));
            }).Start();
        }
    }
}
