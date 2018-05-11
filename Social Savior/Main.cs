using AdvancedBinary;
using System;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32;

namespace Social_Savior {
    public partial class Main : Form {
        public bool FirstLaunch = false;
        Settings Settings = new Settings();

        KeyboardHook PanicHotkey = new KeyboardHook();
        KeyboardHook RestoreHotkey = new KeyboardHook();

        string SettingsPath = AppDomain.CurrentDomain.BaseDirectory + "Savior.dat";
        public Main() {
            InitializeComponent();
            if (SingleInstanceService.PipeIsOpen()) {
                SingleInstanceService.RequestOpen();
                Environment.Exit(0);
            }

            SingleInstanceService.StartPipe();

            if (File.Exists(SettingsPath)) {
                using (Stream ReaderStream = new StreamReader(SettingsPath).BaseStream)
                using (StructReader Reader = new StructReader(ReaderStream)) {
                    Reader.ReadStruct(ref Settings);
                    Reader.Close();
                }

                SetReflexByCode(Settings.ReflexAction);

                PanicAltCK.Checked = Settings.Panic.Alt;
                PanicCtrlCK.Checked = Settings.Panic.Ctrl;
                PanicShiftCK.Checked = Settings.Panic.Shift;
                PanicTB.Text = KeyName((Keys)Settings.Panic.KeyCode);


                RestoreAltCK.Checked = Settings.Restore.Alt;
                RestoreCtrlCK.Checked = Settings.Restore.Ctrl;
                RestoreShiftCK.Checked = Settings.Restore.Shift;
                RestoreTB.Text = KeyName((Keys)Settings.Restore.KeyCode);

                MuteComputerCK.Checked = Settings.MuteAll;
                MuteBlackListCK.Checked = Settings.MuteBlacklist;
                InvokeScreenSaverCK.Checked = Settings.InvokeScreenSaver;
                FocusAProgramCK.Checked = Settings.FocusProcess;

            } else FirstLaunch = true;

            if (Settings.Blacklist == null)
                Settings.Blacklist = new string[0];

            try {
                if ((Keys)Settings.Panic.KeyCode != Keys.None) {
                    PanicHotkey.RegisterHotKey(GetPanicModifiers(), (Keys)Settings.Panic.KeyCode);
                    PanicHotkey.KeyPressed += PanicHotkeyPressed;
                }
            } catch {
                MessageBox.Show("Failed to Register the Hotkey, Please Try a new Panic Hotkey.", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try {
                if ((Keys)Settings.Restore.KeyCode != Keys.None) {
                    RestoreHotkey.RegisterHotKey(GetRestoreModifiers(), (Keys)Settings.Restore.KeyCode);
                    RestoreHotkey.KeyPressed += RestoreHotkeyPressed;
                }
            } catch {
                MessageBox.Show("Failed to Register the Hotkey, Please Try a new Restore Hotkey.", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ProcessScanTick(null, null);
        }

        private void PanicHotkeyPressed(object sender, KeyPressedEventArgs e) {
            if (Visible) {
                lblPanicTest.Text = "Panic!";
                lblPanicTest.BackColor = Color.Red;
            } else {
                //Real Panic - LET'S HIDE THIS SHIT
                Process[] Targets = GetExecutingBlackList();
                string[] LOG = new string[0];
                if (Settings.MuteBlacklist) {
                    foreach (Process Target in Targets) {
                        try {
                            Target.MuteProcess(true);
                        } catch { 
                            AppendArray(ref LOG, "Failed to mute: " + Target.ProcessName);
                        }
                    }
                }

                if (Settings.MuteAll)
                    AudioController.AudioManager.SetMasterVolumeMute(true);

                if (HideWindowRatio.Checked) {
                    foreach (Process Target in Targets) {
                        try {
                            Target.HideMainWindow();
                        } catch {
                            AppendArray(ref LOG, "Failed to hide: " + Target.ProcessName);
                        }
                    }
                }
                if (KillProcessRadio.Checked) {
                    foreach (Process Target in Targets) {
                        try {
                            Target.Kill();
                        } catch {
                            AppendArray(ref LOG, "Failed to kill: " + Target.ProcessName);
                        }
                    }
                }
                if (SuspendProcessRadio.Checked) {
                    foreach (Process Target in Targets) {
                        try {
                            Target.HideMainWindow();
                            Thread.Sleep(10);
                            Target.SuspendProcess();
                        } catch { AppendArray(ref LOG, "Failed to suspend: " + Target.ProcessName); }
                    }
                }

                if (Settings.InvokeScreenSaver) {
                    RegistryKey screenSaverKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
                    if (screenSaverKey != null) {
                        string screenSaverFilePath = screenSaverKey.GetValue("SCRNSAVE.EXE", string.Empty).ToString();
                        if (!string.IsNullOrEmpty(screenSaverFilePath) && File.Exists(screenSaverFilePath)) {
                            Process screenSaverProcess = System.Diagnostics.Process.Start(new ProcessStartInfo(screenSaverFilePath, "/s"));  // "/s" for full-screen mode
                        }
                    }
                }

                if (Settings.FocusProcess) {
                    var Processes = System.Diagnostics.Process.GetProcessesByName(Settings.ProcessToFocus);
                    if (Processes.Length > 0) {
                        foreach (Process Process in Processes) {
                            if (Process.MainWindowHandle != IntPtr.Zero) {
                                Process.FocusMainWindow();
                                break;
                            }
                        }
                    }
                }
            }
        }
        private void RestoreHotkeyPressed(object sender, KeyPressedEventArgs e) {
            if (Visible) {
                lblPanicTest.Text = "Not in Panic";
                lblPanicTest.BackColor = Color.Lime;
            } else {
                //Real Restore
                Process[] Targets = GetExecutingBlackList();

                if (Settings.MuteBlacklist) {
                    foreach (Process Target in Targets) {
                        Target.MuteProcess(false);
                    }
                }

                if (Settings.MuteAll)
                    AudioController.AudioManager.SetMasterVolumeMute(false);

                if (HideWindowRatio.Checked) {
                    foreach (Process Target in Targets)
                        Target.ShowMainWindow();
                }                
                if (SuspendProcessRadio.Checked) {
                    foreach (Process Target in Targets) {
                        try {
                            Target.ResumeProcess();
                            Thread.Sleep(10);
                            Target.ShowMainWindow();
                        } catch { }
                    }
                }
            }
        }

        private Process[] GetExecutingBlackList() {
            return (from x in System.Diagnostics.Process.GetProcesses() where Settings.Blacklist.Contains(x.ProcessName) select x).ToArray();
        }

        private void PanicKeyDown(object sender, KeyEventArgs e) {
            e.SuppressKeyPress = true;

            PanicTB.Text = KeyName(e.KeyData);
            if (PanicTB.Text.Contains(",")) {
                MessageBox.Show("Invalid Hotkey", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PanicTB.Text = KeyName((Keys)Settings.Panic.KeyCode);
                return;
            }

            Settings.Panic.KeyCode = (int)e.KeyData;
            UpdatePanicHotKey();
        }

        private void RestoreKeyDown(object sender, KeyEventArgs e) {
            e.SuppressKeyPress = true;

            RestoreTB.Text = KeyName(e.KeyData);
            if (RestoreTB.Text.Contains(",")) {
                MessageBox.Show("Invalid Hotkey", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RestoreTB.Text = KeyName((Keys)Settings.Restore.KeyCode);
                return;
            }

            Settings.Restore.KeyCode = (int)e.KeyData;
            UpdateRestoreHotKey();
        }

        public byte GetReflexCode() {
            if (SuspendProcessRadio.Checked)
                return 0;
            if (HideWindowRatio.Checked)
                return 1;

            return 2;//Kill Process
        }

        public void SetReflexByCode(byte b) {
            switch (b) {
                case 0:
                    SuspendProcessRadio.Checked = true;
                    break;
                case 1:
                    HideWindowRatio.Checked = true;
                    break;
                case 2:
                    KillProcessRadio.Checked = true;
                    break;
            }
        }

        public void UpdateExtraReactions(object sender, EventArgs e) {
            Settings.MuteAll = MuteComputerCK.Checked;
            Settings.MuteBlacklist = MuteBlackListCK.Checked;
            Settings.InvokeScreenSaver = InvokeScreenSaverCK.Checked;

            Settings.FocusProcess = FocusAProgramCK.Checked;
            if (Settings.FocusProcess && string.IsNullOrEmpty(Settings.ProcessToFocus)) {
                var ProcSelector = new ProcessPicker();
                if (ProcSelector.ShowDialog() != DialogResult.OK) {
                    Settings.FocusProcess = false;
                    FocusAProgramCK.Checked = false;
                    return;
                }
                Settings.ProcessToFocus = ProcSelector.SelectedProcess.ProcessName;
            }
        }

        public void UpdatePanicHotKey() {
            PanicHotkey.Dispose();
            System.Threading.Thread.Sleep(100);
            PanicHotkey = new KeyboardHook();
            try {
                PanicHotkey.RegisterHotKey(GetPanicModifiers(), (Keys)Settings.Panic.KeyCode);
                PanicHotkey.KeyPressed += PanicHotkeyPressed;
            } catch {
                MessageBox.Show("Failed to Register this Hotkey", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void UpdateRestoreHotKey() {
            RestoreHotkey.Dispose();
            Thread.Sleep(100);
            RestoreHotkey = new KeyboardHook();
            try {
                RestoreHotkey.RegisterHotKey(GetRestoreModifiers(), (Keys)Settings.Restore.KeyCode);
                RestoreHotkey.KeyPressed += RestoreHotkeyPressed;
            } catch {
                MessageBox.Show("Failed to Register this Hotkey", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public ModifierKeys GetPanicModifiers() {
            ModifierKeys Modifiers = new ModifierKeys();
            if (Settings.Panic.Ctrl)
                Modifiers |= Social_Savior.ModifierKeys.Control;
            if (Settings.Panic.Alt)
                Modifiers |= Social_Savior.ModifierKeys.Alt;
            if (Settings.Panic.Shift)
                Modifiers |= Social_Savior.ModifierKeys.Shift;
            if (Settings.Panic.Win)
                Modifiers |= Social_Savior.ModifierKeys.Win;

            return Modifiers;
        }
        public ModifierKeys GetRestoreModifiers() {
            ModifierKeys Modifiers = new ModifierKeys();
            if (Settings.Restore.Ctrl)
                Modifiers |= Social_Savior.ModifierKeys.Control;
            if (Settings.Restore.Alt)
                Modifiers |= Social_Savior.ModifierKeys.Alt;
            if (Settings.Restore.Shift)
                Modifiers |= Social_Savior.ModifierKeys.Shift;
            if (Settings.Restore.Win)
                Modifiers |= Social_Savior.ModifierKeys.Win;

            return Modifiers;
        }
        public string KeyName(Keys Key) {
            return Key.ToString();
        }

        private void SaveSettings() {
            Settings.Blacklist = Settings.Blacklist.Distinct().ToArray();

            using (Stream WriterStream = new StreamWriter(SettingsPath).BaseStream)
            using (StructWriter Writer = new StructWriter(WriterStream)) {
                Writer.WriteStruct(ref Settings);
                Writer.Close();
            }
        }
        private void OnClosing(object sender, FormClosingEventArgs e) {
            SaveSettings();   

            if (FirstLaunch) {
                FirstLaunch = false;

                MessageBox.Show("Well... If you click in the 'x' just hide the Social Savior, if you want open it again just open the Social Savior again and type the secret key.\nIf you really want close the social savior click in the 'Close the Social Savior' button in the main window or kill the process.\nI hope this save your file, Social Savior, Created by Marcussacana.", "Welcome to the Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            e.Cancel = true;
            Visible = false;
        }

        private void PanicCtrlClicked(object sender, EventArgs e) {
            Settings.Panic.Ctrl = PanicCtrlCK.Checked;
            UpdatePanicHotKey();
        }

        private void PanicAltClicked(object sender, EventArgs e) {
            Settings.Panic.Alt = PanicAltCK.Checked;
            UpdatePanicHotKey();
        }

        private void PanicShiftClicked(object sender, EventArgs e) {
            Settings.Panic.Shift = PanicShiftCK.Checked;
            UpdatePanicHotKey();
        }

        private void RestoreCtrlClicked(object sender, EventArgs e) {
            Settings.Restore.Ctrl = RestoreCtrlCK.Checked;
            UpdateRestoreHotKey();
        }

        private void RestoreAltClicked(object sender, EventArgs e) {
            Settings.Restore.Alt = RestoreAltCK.Checked;
            UpdateRestoreHotKey();
        }

        private void RestoreShiftClicked(object sender, EventArgs e) {
            Settings.Restore.Shift = RestoreShiftCK.Checked;
            UpdateRestoreHotKey();
        }

        private void ReflexChanged(object sender, EventArgs e) {
            Settings.ReflexAction = GetReflexCode();
        }

        private void iTalk_Button_11_Click(object sender, EventArgs e) {
            SaveSettings();

            if (DialogResult.Yes == MessageBox.Show("You have sure want exit of the Social Savior?\nThis will disable the Panic Button.", "Social Savior", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk)) {
                PanicHotkey.Dispose();
                RestoreHotkey.Dispose();

                try {
                    Environment.Exit(0);
                } catch {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            }
        }

        private void ProcessScanTick(object sender, EventArgs e) {
            var Processes = (from x in System.Diagnostics.Process.GetProcesses() where !string.IsNullOrWhiteSpace(x.MainWindowTitle) select x).ToArray();

            ProcessList.Items.Clear();
            foreach (Process Process in Processes) {
                ProcessList.Items.Add(new ListViewItem(new string[] { Process.ProcessName, Settings.Blacklist.Contains(Process.ProcessName) ? "Yes" : "No", Process.MainWindowTitle }));
            }

            foreach (string ProcessName in Settings.Blacklist) {
                bool InList = false;
                for (int i = 0; i < ProcessList.Items.Count; i++) {
                    if (ProcessList.Items[i].Text == ProcessName)
                        InList = true;
                }
                if (InList)
                    continue;

                ProcessList.Items.Add(new ListViewItem(new string[] { ProcessName, "Yes", "Program Not Running..."}));
            }
        }


        private void OnBlackListMenuOpen(object sender, System.ComponentModel.CancelEventArgs e) {
            bool ItemSelected = Blacklist.ListView.SelectedItems.Count > 0;
            if (!ItemSelected)
                e.Cancel = true;
        }

        private void AddToBlackList_Click(object sender, EventArgs e) {
            for (int i = 0; i < Blacklist.ListView.SelectedItems.Count; i++) {
                var Item = Blacklist.ListView.SelectedItems[i];
                AppendArray(ref Settings.Blacklist, Item.Text);
            }

            ProcessScanTick(null, null);
        }
        private void DelOfTheBlackList_Click(object sender, EventArgs e) {
            for (int i = 0; i < Blacklist.ListView.SelectedItems.Count; i++) {
                var Item = Blacklist.ListView.SelectedItems[i];
                RemoveOfArray(ref Settings.Blacklist, Item.Text);
            }

            ProcessScanTick(null, null);
        }

        private void AppendArray<T>(ref T[] Array, T Item) => AppendArray(ref Array, new T[] { Item });
        private void AppendArray<T>(ref T[] Array, T[] Items) {
            if (Array == null)
                Array = new T[0];

            T[] NewArray = new T[Array.Length + Items.Length];
            Array.CopyTo(NewArray, 0);
            Items.CopyTo(NewArray, Array.Length);
            Array = NewArray;
        }
        private void RemoveOfArray<T>(ref T[] Array, T Item) => RemoveOfArray(ref Array, new T[] { Item });
        private void RemoveOfArray<T>(ref T[] Array, T[] Items) {
            if (Array == null)
                return;

            List<T> Result = new List<T>();
            foreach (T Item in Array) {
                if (Items.Contains(Item))
                    continue;
                else
                    Result.Add(Item);
            }

            Array = Result.ToArray();
        }
    }
    public struct Settings {
        [StructField]
        public Hotkey Panic;

        [StructField]
        public Hotkey Restore;

        public byte ReflexAction;

        public bool MuteAll;
        public bool MuteBlacklist;
        public bool InvokeScreenSaver;
        public bool FocusProcess;
        [CString]
        public string ProcessToFocus;

        [PArray(PrefixType = Const.UINT32), CString]
        public string[] Blacklist;
    }

    public struct Hotkey {
        public bool Ctrl;
        public bool Shift;
        public bool Alt;
        public bool Win;

        public int KeyCode;
    }
}

