﻿using AdvancedBinary;
using System;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using System.Threading.Tasks;
using WindowsStartup.Utils;
using System.Runtime.InteropServices;

namespace Social_Savior {
    public partial class Main : Form {

        const string Signature = "SSAV";
        const uint SignatureValue = 0x56415353;
        const ushort SettingsVersion = 3;

        public bool Initialized = false;
        public bool FirstLaunch = false;
        Settings Settings = new Settings();

        KeyboardHook PanicHotkey = new KeyboardHook();
        KeyboardHook RestoreHotkey = new KeyboardHook();
        KeyboardHook IDLEHotkey = new KeyboardHook();

        MMDevice Microphone = null;
        bool LevelSetup = false;
        byte NewLevel = 0;

        public static string SettingsPath = AppDomain.CurrentDomain.BaseDirectory + "Savior.dat";
        public Main() {
            if (File.Exists(SettingsPath)) {
                try {
                    using (Stream ReaderStream = new StreamReader(SettingsPath).BaseStream)
                    using (StructReader Reader = new StructReader(ReaderStream)) {
                        if (Reader.ReadUInt32() != SignatureValue) {
                            Reader.Close();
                            BadSettings();
                        }
                        ushort Ver = Reader.ReadUInt16();


                        if (Ver < SettingsVersion) {
                            try {
                                Reader.BaseStream.Position = 0;
                                Reader.ReadStruct(ref Settings);
                            } catch { }
                            Settings.Version = SettingsVersion;
                        } else if (Ver != SettingsVersion) {
                            Reader.Close();
                            BadSettings();
                        } else {
                            Reader.BaseStream.Position = 0;
                            Reader.ReadStruct(ref Settings);
                            Reader.Close();
                        }
                    }
                } catch {
                    BadSettings();
                }
            }

            if (Settings.AllowedUser == string.Empty || Settings.AllowedUser == null) {
                var Reply = MessageBox.Show("Allow the Social-Savior works only with the current user of this computer?", "Social Savior", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (Reply == DialogResult.Yes)
                    Settings.AllowedUser = Environment.UserName;
                else
                    Settings.AllowedUser = "ALLOWEVERYONE";
            } else if (Settings.AllowedUser != Environment.UserName && Settings.AllowedUser != "ALLOWEVERYONE")
                Environment.Exit(0);

            InitializeComponent();

            if (SingleInstanceService.PipeIsOpen() && !Debugger.IsAttached) {
                SingleInstanceService.RequestOpen();
                Environment.Exit(0);
            }

            SingleInstanceService.StartPipe();

            if (File.Exists(SettingsPath)) {
                SetReflexByCode(Settings.ReflexAction);

                PanicAltCK.Checked = Settings.Panic.Alt;
                PanicCtrlCK.Checked = Settings.Panic.Ctrl;
                PanicShiftCK.Checked = Settings.Panic.Shift;
                PanicTB.Text = KeyName((Keys)Settings.Panic.KeyCode);


                RestoreAltCK.Checked = Settings.Restore.Alt;
                RestoreCtrlCK.Checked = Settings.Restore.Ctrl;
                RestoreShiftCK.Checked = Settings.Restore.Shift;
                RestoreTB.Text = KeyName((Keys)Settings.Restore.KeyCode);

                ckIDLEAlt.Checked = Settings.AllowIdle.Alt;
                ckIDLECtrl.Checked = Settings.AllowIdle.Ctrl;
                ckIDLEShift.Checked = Settings.AllowIdle.Shift;
                tbIDLE.Text = KeyName((Keys)Settings.AllowIdle.KeyCode);


                MuteComputerCK.Checked = Settings.MuteAll;
                MuteBlackListCK.Checked = Settings.MuteBlacklist;
                InvokeScreenSaverCK.Checked = Settings.InvokeScreenSaver;
                FocusAProgramCK.Checked = Settings.FocusProcess;
                KillProcIfFailCK.Checked = Settings.KillProcessIfFails;

                MaxIDLE.Value = Settings.MaxIDLE;

            } else FirstLaunch = true;

            if (Settings.Blacklist == null)
                Settings.Blacklist = new string[0];

            try {
                if ((Keys)Settings.Panic.KeyCode != Keys.None) {
                    PanicHotkey.RegisterHotKey(GetPanicModifiers(), (Keys)Settings.Panic.KeyCode);
                    PanicHotkey.KeyPressed += PanicHotkeyPressed;
                }
            } catch {
                MessageBox.Show("Failed to Register hotkey, Please Try a new Panic Hotkey.", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try {
                if ((Keys)Settings.Restore.KeyCode != Keys.None) {
                    RestoreHotkey.RegisterHotKey(GetRestoreModifiers(), (Keys)Settings.Restore.KeyCode);
                    RestoreHotkey.KeyPressed += RestoreHotkeyPressed;
                }
            } catch {
                MessageBox.Show("Failed to Register hotkey, Please Try a new Restore Hotkey.", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try {
                if ((Keys)Settings.AllowIdle.KeyCode != Keys.None) {
                    IDLEHotkey.RegisterHotKey(GetIDLEModifiers(), (Keys)Settings.AllowIdle.KeyCode);
                    IDLEHotkey.KeyPressed += IDLEHotkeyPressed;
                }
            } catch {
                MessageBox.Show("Failed to Register hotkey, Please Try a new Restore Hotkey.", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MicroLevel.ColorList = new List<Color> {
                Color.Lime,
                Color.Yellow,
                Color.Red
            };
            
            MainContainer.Text = "Social Savior - v" + GitHub.CurrentVersion;

            if (FirstLaunch)
                HomeMainGB.Text = "Social Savior has not been configured";

            InitializeMicrophone();
            ProcessScanTick(null, null);

            WineDeprecated();

            StartWithWindowsCK.Checked = StartupStatus();

            if (Program.Startup) {
               Shown += (a, b) => Close();
            } else Focus();

            Initialized = true;
        }

        private void WineDeprecated()
        {
            if (Program.IsRealWindows)
                return;

            InvokeScreenSaverCK.Enabled = false;
            InvokeScreenSaverCK.Checked = false;
            StartWithWindowsCK.Enabled = false;
            Microphone = null;
        }

        private void InitializeMicrophone() {
            if (Program.IsRealWindows) {
                MMDeviceCollection Devices = GetMicrophoneDevices();
                var DevicesName = (from x in Devices select x.DeviceFriendlyName).ToArray();
                MicroList.Items.AddRange(DevicesName);

                if (!string.IsNullOrEmpty(Settings.RecoderDevice) && DevicesName.Contains(Settings.RecoderDevice))
                {
                    MicroList.SelectedItem = Settings.RecoderDevice;
                }
            }
            MicroList.Items.Add("Disabled");
        }

        private MMDeviceCollection GetMicrophoneDevices() {
            MMDeviceEnumerator Enumerator = new MMDeviceEnumerator();
            return Enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        }

        private void BadSettings() {
            if (DialogResult.Yes == MessageBox.Show("The Settings file is incompatible with this version of Social Savior,\nDo you want reset all settings?\n(If you don't, the program will exit.)", "Social Savior", MessageBoxButtons.YesNo, MessageBoxIcon.Hand)) {
                File.Delete(SettingsPath);
                Application.Restart();
            } else {
                Environment.Exit(0);
            }
        }

        static bool InPanic = false;
        static bool PanicRunning = false;
        static bool RestoreRunning = false;
        internal void PanicHotkeyPressed(object sender, KeyPressedEventArgs e) {
            if (PanicRunning)
                return;
            InPanic = true;
            PanicRunning = true;
            try {
                if (Visible) {
                    lblPanicTest.Text = "Panic!";
                    lblPanicTest.BackColor = Color.Red;
                } else {
                    Program.Log("EVENT: Panic Triggered");

                    //Real Panic - LET'S HIDE THIS SHIT
                    Process[] Targets = GetExecutingBlackList();
                    string[] LOG = new string[0];
                    if (Settings.MuteBlacklist) {
                        foreach (var Target in Targets)
                            SafeInvoker(() => Target.MuteProcess(true), () => {
                                if (Settings.KillProcessIfFails)
                                    Target.Kill();
                            });
                    }

                    if (Settings.MuteAll)
                        AudioController.AudioManager.SetMasterVolumeMute(true);

                    if (HideWindowRatio.Checked) {
                        foreach (var Target in Targets)
                            SafeInvoker(Target.HideWindow, () => {
                                if (Settings.KillProcessIfFails)
                                    Target.Kill();
                            });
                    }
                    if (KillProcessRadio.Checked) {
                        foreach (var Target in Targets)
                            SafeInvoker(Target.Kill);
                    }
                    if (SuspendProcessRadio.Checked) {
                        foreach (var Target in Targets)
                            SafeInvoker(() => {
                                Target.HideWindow();
                                Target.SuspendProcess();
                            }, () => {
                                if (Settings.KillProcessIfFails)
                                    Target.Kill();
                            });
                    }

                    if (Settings.InvokeScreenSaver) {
                        RegistryKey screenSaverKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
                        bool Lock = false;
                        if (screenSaverKey != null) {
                            string screenSaverFilePath = screenSaverKey.GetValue("SCRNSAVE.EXE", string.Empty).ToString();
                            if (!string.IsNullOrEmpty(screenSaverFilePath) && File.Exists(screenSaverFilePath)) {
                                System.Diagnostics.Process.Start(new ProcessStartInfo(screenSaverFilePath, "/s"));  // "/s" for full-screen mode
                            } else Lock = true;
                        } else Lock = true;

                        if (Lock) {
                            LockWorkStation();
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
            } catch (Exception ex) {
                Program.Log(ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source);
            }
            PanicRunning = false;
        }
        internal void RestoreHotkeyPressed(object sender, KeyPressedEventArgs e) {
            if (RestoreRunning)
                return;
            InPanic = false;
            RestoreRunning = true;
            try {
                if (Visible) {
                    lblPanicTest.Text = "Not in Panic";
                    lblPanicTest.BackColor = Color.Lime;
                } else {
                    Program.Log("EVENT: Panic Restation Triggered");

                    //Real Restore
                    Process[] Targets = GetExecutingBlackList();

                    if (Settings.MuteBlacklist) {
                        foreach (Process Target in Targets) {
                            SafeInvoker(() => Target.MuteProcess(false));
                        }
                    }

                    if (Settings.MuteAll)
                        SafeInvoker(() => AudioController.AudioManager.SetMasterVolumeMute(false));

                    if (HideWindowRatio.Checked) {
                        foreach (Process Target in Targets)
                            SafeInvoker(Target.ShowWindow);
                    }
                    if (SuspendProcessRadio.Checked) {
                        foreach (Process Target in Targets) {
                            SafeInvoker(() => {
                                Target.ResumeProcess();
                                Target.ShowWindow();
                            });
                        }
                    }
                }
            } catch (Exception ex) {
                Program.Log(ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source);
            }
            RestoreRunning = false;
        }

        private void IDLEHotkeyPressed(object sender, KeyPressedEventArgs e) {
            if (Visible) {
                lblPanicTest.Text = "IDLE\nPaused";
                lblPanicTest.BackColor = Color.Yellow;
            }
            UnchangedSeconds = 0;
            ChangedSeconds = 0;
            Suspended = false;
            IDLEWatcher.Enabled = false;
            IDLESuspension.Enabled = true;
        }

        private Process[] GetExecutingBlackList() {
            return (from x in System.Diagnostics.Process.GetProcesses() where Settings.Blacklist.Contains(x.ProcessName) select x).ToArray();
        }

        private void PanicKeyDown(object sender, KeyEventArgs e) {
            e.SuppressKeyPress = true;

            PanicTB.Text = KeyName(e.KeyCode);
            if (PanicTB.Text.Contains(",")) {
                MessageBox.Show("Invalid Hotkey", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PanicTB.Text = KeyName((Keys)Settings.Panic.KeyCode);
                return;
            }

            Settings.Panic.KeyCode = (int)e.KeyCode;
            UpdatePanicHotKey();
        }

        private void RestoreKeyDown(object sender, KeyEventArgs e) {
            e.SuppressKeyPress = true;

            RestoreTB.Text = KeyName(e.KeyCode);
            if (RestoreTB.Text.Contains(",")) {
                MessageBox.Show("Invalid Hotkey", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RestoreTB.Text = KeyName((Keys)Settings.Restore.KeyCode);
                return;
            }

            Settings.Restore.KeyCode = (int)e.KeyCode;
            UpdateRestoreHotKey();
        }


        private void IDLE_KeyDown(object sender, KeyEventArgs e) {
            e.SuppressKeyPress = true;

            tbIDLE.Text = KeyName(e.KeyCode);
            if (tbIDLE.Text.Contains(",")) {
                MessageBox.Show("Invalid Hotkey", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Error);
                tbIDLE.Text = KeyName((Keys)Settings.Restore.KeyCode);
                return;
            }

            Settings.AllowIdle.KeyCode = (int)e.KeyCode;
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
            Settings.KillProcessIfFails = KillProcIfFailCK.Checked;

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
            Thread.Sleep(100);
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
        public void UpdateIDLEHotKey() {
            IDLEHotkey.Dispose();
            Thread.Sleep(100);
            IDLEHotkey = new KeyboardHook();
            try {
                IDLEHotkey.RegisterHotKey(GetIDLEModifiers(), (Keys)Settings.AllowIdle.KeyCode);
                IDLEHotkey.KeyPressed += RestoreHotkeyPressed;
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

        public ModifierKeys GetIDLEModifiers() {
            ModifierKeys Modifiers = new ModifierKeys();
            if (Settings.AllowIdle.Ctrl)
                Modifiers |= Social_Savior.ModifierKeys.Control;
            if (Settings.AllowIdle.Alt)
                Modifiers |= Social_Savior.ModifierKeys.Alt;
            if (Settings.AllowIdle.Shift)
                Modifiers |= Social_Savior.ModifierKeys.Shift;
            if (Settings.AllowIdle.Win)
                Modifiers |= Social_Savior.ModifierKeys.Win;

            return Modifiers;
        }
        public string KeyName(Keys Key) {
            return Key.ToString();
        }

        private void SaveSettings() {
            Settings.Signature = Signature;
            Settings.Version = SettingsVersion;

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

                MessageBox.Show("Well... If you just click the 'x', Social Savior will be hidden. If you want to unhide it, just open Social Savior again and type the secret key.\nIf you want to actually close Social Savior, click the 'Close Social Savior' button in the main window or kill the process.\nI hope this can save your life.\n\nSocial Savior, Created by Marcussacana.", "Welcome to Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (!Program.IsRealWindows)
                    MessageBox.Show("Looks like you're running the Social Savior under Wine, So pay attention to a few things:\n1 - The Social Savior Don't will works if your current focused window isn't a wine program.\n2 - You can launch the Social Savior with the argument '/panic' or '/restore', this mean you can create a native hotkey shortcut to run the Social Savior with those arguments.\n3 - Microphone Trigger don't works.\n4 - Computer Locker/Screensaver don't work too.\n5 - We don't recommend you use native hotkey to the /panic argument, because can have a big delay to the panic trigger.\n\nIf you feel anything strange, try create a issue in our github.", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
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

        private void IDLECtrlClicked(object sender, EventArgs e) {
            Settings.AllowIdle.Ctrl = ckIDLECtrl.Checked;
            UpdateIDLEHotKey();
        }

        private void IDLEAltClicked(object sender, EventArgs e) {
            Settings.AllowIdle.Alt = ckIDLECtrl.Checked;
            UpdateIDLEHotKey();
        }

        private void IDLEShiftClicked(object sender, EventArgs e) {
            Settings.AllowIdle.Shift = ckIDLECtrl.Checked;
            UpdateIDLEHotKey();
        }

        private void iTalk_Button_11_Click(object sender, EventArgs e) {
            SaveSettings();

            if (DialogResult.Yes == MessageBox.Show("Are you sure you want to exit Social Savior?\nThis will disable the Panic Button.", "Social Savior", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk)) {
                PanicHotkey.Dispose();
                RestoreHotkey.Dispose();
                System.Diagnostics.Process.GetCurrentProcess().Kill();
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

        private void MicroWatcherTick(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace((string)MicroList.SelectedItem))
                return;

            if (Microphone == null) {
                if (!SelectMicrophone())
                    return;
            }

            if (Microphone.State != DeviceState.Active)
                return;

            if (Settings.WarningSoundLevel == 0 && !LevelSetup) {
                lblWarningLevel.Text = "Disabled";
                return;
            }

            int MicroAtualLevel = (int)Math.Round(Microphone.AudioMeterInformation.MasterPeakValue * 100);
            if (Visible)
                MicroLevel.Value = MicroAtualLevel;

            if (LevelSetup) {
                if (MicroAtualLevel >= NewLevel)
                    NewLevel = (byte)MicroAtualLevel;
            } else if (MicroAtualLevel >= Settings.WarningSoundLevel) {
                PanicHotkeyPressed(null, null);
            }

            lblAtualLevel.Text = MicroAtualLevel + "%";
            lblWarningLevel.Text = (LevelSetup ? NewLevel : Settings.WarningSoundLevel) + "%";
        }

        private bool SelectMicrophone() {
            if (!Program.IsRealWindows) {
                MicroWatcher.Enabled = false;
                Microphone = null;
                return false;
            }

            var Devices = (from x in GetMicrophoneDevices()
                           where x.DeviceFriendlyName == Settings.RecoderDevice
                           select x).ToArray();

            if (Devices.Length == 0) {
                MicroWatcher.Interval = 500;
                Microphone = null;
                return false;
            }

            MicroWatcher.Interval = 10;
            Microphone = Devices.First();
            return true;
        }

        private void MicroChanged(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace((string)MicroList.SelectedItem))
                return;

            Settings.RecoderDevice = (string)MicroList.SelectedItem;
            SelectMicrophone();
        }

        private void ChangeAudioTrigger(object sender, EventArgs e) {
            LevelSetup = !LevelSetup;
            SetupMaxLevelBnt.Text = LevelSetup ? "Apply New Level" : "Change Trigger Level";

            if (LevelSetup) {
                if (FirstLaunch)
                    MessageBox.Show("Well... For this feature, put your microphone somewhere near the door, for example...\nThen open the door at a normal speed and let Social Savior mark the noise level, and click \"Apply new level\" to set the noise level as the new threshold.", "Social Savior", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NewLevel = 0;
            } else
                Settings.WarningSoundLevel = NewLevel;
        }

        public static object obj = new object();
        public static void SafeInvoker(Action Action, Action Timeouted = null, int Timeout = 200) {
            lock (obj)
            {
                try
                {
                    Task Async = new Task(Action);
                    Async.Start();
                    if (!Async.Wait(Timeout))
                    {
                        Timeouted?.Invoke();
                    }

                }
                catch (Exception ex)
                {
                    Program.Log("SafeInvoker Exception:\n" + ex.ToString());
                }
            }
        }

        const string AppName = "Social Savior";
        private void StartWindowsClicked(object sender, EventArgs e) {
            string Startup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
            if (StartWithWindowsCK.Checked)
                ShortcutTool.Create(Startup, AppName, Application.ExecutablePath, "/Startup", "A Social Media Picture Backuper Tool");
            else
                if (StartupStatus())
                    File.Delete(ShortcutTool.GetShortcutPath(Startup, AppName));

        }
        private bool StartupStatus() {
            string Startup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
            return File.Exists(ShortcutTool.GetShortcutPath(Startup, AppName));
        }

        private void TriggerLevelClicked(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left && Settings.WarningSoundLevel > 0)
                Settings.WarningSoundLevel--;
            if (e.Button == MouseButtons.Right && Settings.WarningSoundLevel < 100)
                Settings.WarningSoundLevel++;
        }


        [DllImport("user32.dll")]
        public static extern void LockWorkStation();

        [DllImport("user32.dll")]
        public static extern bool GetLastInputInfo(ref LAST_INPUT_INFO plii);

        public struct LAST_INPUT_INFO {
            public uint cbSize;
            public uint dwTime;
        }

        private void MaxIDLEUpdated(object sender, EventArgs e) {
            Settings.MaxIDLE = (ushort)MaxIDLE.Value;

            if (Initialized) {
                if (Settings.MaxIDLE == 0 && Visible) {
                    lblPanicTest.Text = "IDLE\nOFF";
                    lblPanicTest.BackColor = Color.Yellow;
                } else {
                    lblPanicTest.Text = "IDLE\nON";
                    lblPanicTest.BackColor = Color.Lime;
                }
            }
        }

        private uint UnchangedSeconds;
        private uint ChangedSeconds;
        private uint LastInputValue;
        private bool Suspended;
        private void WatchIDLE(object sender, EventArgs e) {
            if (Settings.MaxIDLE == 0) {
                LbIDLEMin.Text = "Min";
                return;
            }

            var LII = new LAST_INPUT_INFO() {
                cbSize = (uint)Marshal.SizeOf(typeof(LAST_INPUT_INFO))
            };
            if (!GetLastInputInfo(ref LII)) {
                LbIDLEMin.Text = "Min; ERROR";
                return;
            }

            uint LIT = LII.dwTime;
            if (LIT != LastInputValue) {
                UnchangedSeconds = 0;
                LastInputValue = LIT;
            } else
                UnchangedSeconds++;

            uint UnchangedMin = UnchangedSeconds / 60;
            if (UnchangedMin >= Settings.MaxIDLE && !InPanic)
                PanicHotkeyPressed(null, null);


            LbIDLEMin.Text = string.Format("Min; IDLE: {1}s", UnchangedMin, UnchangedSeconds);
        }

        private void IdleSuspension(object sender, EventArgs e) {
            LbIDLEMin.Text = "Min";
            if (Settings.MaxIDLE == 0)
                return;

            var LII = new LAST_INPUT_INFO() {
                cbSize = (uint)Marshal.SizeOf(typeof(LAST_INPUT_INFO))
            };
            if (!GetLastInputInfo(ref LII)) {
                LbIDLEMin.Text = "Min; ERROR";
                return;
            }

            uint LIT = LII.dwTime;
            if (LIT != LastInputValue) {
                UnchangedSeconds = 0;
                LastInputValue = LIT;
            } else
                UnchangedSeconds++;


            if (!Suspended && UnchangedSeconds > 5)
                Suspended = true;
            else if (!Suspended)
                LbIDLEMin.Text = string.Format("Min; Not Suspended ({0}s of 6s)", UnchangedSeconds);
            else { 
                if (ChangedSeconds > 4) {
                    LbIDLEMin.Text = "Min";
                    Suspended = false;
                    IDLESuspension.Enabled = false;
                    IDLEWatcher.Enabled = true;

                    if (Visible) {
                        lblPanicTest.Text = "IDLE\nRunning";
                        lblPanicTest.BackColor = Color.Lime;
                    }
                } else if (UnchangedSeconds <= 1)
                    ChangedSeconds++;
                 else
                    ChangedSeconds = 0;

                LbIDLEMin.Text = string.Format("Min; Suspended ({0}s of 5s)", ChangedSeconds);
            }
        }
    }
    public struct Settings {
        [FString(Length = 4)]
        public string Signature;

        public ushort Version;

        [StructField]
        public Hotkey Panic;

        [StructField]
        public Hotkey Restore;

        public byte ReflexAction;

        public bool MuteAll;
        public bool MuteBlacklist;
        public bool InvokeScreenSaver;
        public bool FocusProcess;
        public bool KillProcessIfFails;

        [CString]
        public string ProcessToFocus;

        [PArray(PrefixType = Const.UINT32), CString]
        public string[] Blacklist;

        [CString]
        public string RecoderDevice;
        public byte WarningSoundLevel;

        public ushort MaxIDLE;

        [StructField]
        public Hotkey AllowIdle;

        [CString()]
        public string AllowedUser;
    }

    public struct Hotkey {
        public bool Ctrl;
        public bool Shift;
        public bool Alt;
        public bool Win;

        public int KeyCode;
    }
}

