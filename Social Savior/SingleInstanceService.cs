using System.Diagnostics;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Social_Savior {
    public static class SingleInstanceService {
        public static bool PipeIsOpen() {
            return Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath)).Length > 1;
        }

        public static void StartPipe() {
            new Task(() => {
                while (true) {
                    try {
                        using (NamedPipeServerStream Server = new NamedPipeServerStream("SocialSavior")) {
                            Server.WaitForConnection();
                            int b = Server.ReadByte();
                            switch (b) {
                                case 0:
                                    Program.MainFormInstance.Invoke(new MethodInvoker(() => {
                                        Program.MainFormInstance.Visible = true;
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

        public static void RequestOpen() {
            using (NamedPipeClientStream Client = new NamedPipeClientStream("SocialSavior")) {
                Client.Connect();
                Client.WriteByte(0);
                Client.Close();
            }
        }
    }
}
