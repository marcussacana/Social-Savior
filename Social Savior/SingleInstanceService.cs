using System.Diagnostics;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Social_Savior {
    public static class SingleInstanceService {

        enum InstanceEvent : byte
        {
            Open, Quit, Panic, Restore
        }

        public static bool PipeIsOpen() {
            string Name = System.IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            var Procs = Process.GetProcessesByName(Name);
            return Procs.Length > 1;
        }

        public static void StartPipe() {
            new Task(() => {
                while (true) {
                    try {
                        using (NamedPipeServerStream Server = new NamedPipeServerStream("SocialSavior")) {
                            Server.WaitForConnection();
                            int b = Server.ReadByte();
                            switch ((InstanceEvent)b) {
                                case InstanceEvent.Open:
                                    Program.MainFormInstance.Invoke(new MethodInvoker(() => {
                                        Program.MainFormInstance.Visible = true;
                                    }));
                                    break;
                                case InstanceEvent.Quit:
                                    System.Environment.Exit(0);
                                    break;
                                case InstanceEvent.Panic:
                                    Program.MainFormInstance.Invoke(new MethodInvoker(() => {
                                        var Main = (Main)Program.MainFormInstance;
                                        Main.PanicHotkeyPressed(null, null);
                                    }));
                                    break;
                                case InstanceEvent.Restore:
                                    Program.MainFormInstance.Invoke(new MethodInvoker(() => {
                                        var Main = (Main)Program.MainFormInstance;
                                        Main.RestoreHotkeyPressed(null, null);
                                    }));
                                    break;
                            }
                            Server.Close();
                        }
                    } catch { }
                    System.Threading.Thread.Sleep(100);
                }
                    
                
            }).Start();
        }

        public static void RequestQuit() {
            Main.SafeInvoker(() => {
                        using (NamedPipeClientStream Client = new NamedPipeClientStream("SocialSavior")) {
                            Client.Connect();
                            Client.WriteByte((byte)InstanceEvent.Quit);
                            Client.Close();
                        }
                }, Timeout: 1000);
        }
        public static void RequestOpen() {
            using (NamedPipeClientStream Client = new NamedPipeClientStream("SocialSavior")) {
                Client.Connect();
                Client.WriteByte((byte)InstanceEvent.Open);
                Client.Close();
            }
        }
        public static void RequestPanic() {
            using (NamedPipeClientStream Client = new NamedPipeClientStream("SocialSavior")) {
                Client.Connect();
                Client.WriteByte((byte)InstanceEvent.Panic);
                Client.Close();
            }
        }
        public static void RequestRestore() {
            using (NamedPipeClientStream Client = new NamedPipeClientStream("SocialSavior")) {
                Client.Connect();
                Client.WriteByte((byte)InstanceEvent.Restore);
                Client.Close();
            }
        }
    }
}
