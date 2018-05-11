using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Social_Savior {
    public partial class ProcessPicker : Form {

        public Process SelectedProcess;
        public ProcessPicker() {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
            UpdateTick(null, null);
        }

        private void UpdateTick(object sender, EventArgs e) {
            var Processes = (from x in Process.GetProcesses() where !string.IsNullOrWhiteSpace(x.MainWindowTitle) select x).ToArray();

            ProcList.Items.Clear();
            foreach (Process Process in Processes) {
                ProcList.Items.Add(new ListViewItem(new string[] { Process.ProcessName, Process.MainWindowTitle }));
            }
        }

        private void ProcessSelected(object sender, EventArgs e) {
            if (ProcList.SelectedItems.Count == 0)
                return;
            
            var Processes = (from x in Process.GetProcesses()
                             where !string.IsNullOrWhiteSpace(x.MainWindowTitle) && 
                             x.ProcessName == ProcList.SelectedItems[0].Text.ToString()
                             select x).ToArray();

            SelectedProcess = Processes.First();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
