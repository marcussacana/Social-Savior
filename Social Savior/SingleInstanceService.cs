﻿using System.Diagnostics;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Social_Savior {
    public static class SingleInstanceService {
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
                            switch (b) {
                                case 0:
                                    Program.MainFormInstance.Invoke(new MethodInvoker(() => {
                                        Program.MainFormInstance.Visible = true;
                                    }));
                                    break;
                                case 1:
                                    System.Environment.Exit(0);
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
                            Client.WriteByte(1);
                            Client.Close();
                        }
                }, Timeout: 1000);
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
