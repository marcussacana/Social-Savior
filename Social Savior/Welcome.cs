using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Social_Savior {
    public partial class Welcome : Form {
        string InitialLog, InitialPass;
        bool ReadOnly = false;
        string InputLog, InputPass;
        public Welcome() {
            InitializeComponent();

            InitialLog = EmailTB.Text;
            InitialPass = PassTB.Text;

            Focus();
        }

        private void OnLoginFocused(object sender, EventArgs e) {
            if (EmailTB.Text == InitialLog)
                EmailTB.Text = string.Empty;
        }

        private void OnPassFocused(object sender, EventArgs e) {
            if (PassTB.Text == InitialPass)
                PassTB.Text = string.Empty;
        }

        private void OnLoginDefocused(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(EmailTB.Text))
                EmailTB.Text = InitialLog;
        }

        private void PassChanged(object sender, EventArgs e) {
            if (ReadOnly && PassTB.Text != InputPass)
                PassTB.Text = InputPass;
        }

        private void EmailChanged(object sender, EventArgs e) {
            if (ReadOnly && EmailTB.Text != InputLog)
                EmailTB.Text = InputLog;
        }

        private void OnPassDefocused(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(PassTB.Text))
                PassTB.Text = InitialPass;
        }

        private void LoginClicked(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(PassTB.Text) || string.IsNullOrWhiteSpace(LoginBNT.Text)) {
                MessageBox.Show("Please, Type your email and password to continue", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            InputPass = PassTB.Text;
            InputLog = EmailTB.Text;
            ReadOnly = true;
            string Input = InputPass.Trim().ToLower().Replace(" ", "").Replace("'", "");
            string[] ValidInputs = new string[] {
                "safeidleness", "idlenesssafe", //Do you want to pretend to be working? You can also use this! (Your lazy)
                "safefap", "fapsafe",
                "nsfw2sfw", "nsfwtosfw"
            };
            if (ValidInputs.Contains(Input)) {
                Program.Execute = true;
                Close();
            } else new Task(() => {
                System.Threading.Thread.Sleep(3000);
                Invoke(new MethodInvoker(() => {
                    MessageBox.Show("Failed to connect, Please check your internet or firewall settings.", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }));
            }).Start();

        }
    }
}
