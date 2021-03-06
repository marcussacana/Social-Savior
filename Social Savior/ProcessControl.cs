﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Social_Savior {
    static class ProcessControl {

        static Dictionary<int, List<IntPtr>> HandlerMap = new Dictionary<int, List<IntPtr>>();
        static Dictionary<int, List<ProcessThread>> ThreadMap = new Dictionary<int, List<ProcessThread>>();

        const int SW_HIDE = 0;
        const int SW_SHOWNORMAL = 1;
        const int SW_SHOWMINIMIZED = 2;
        const int SW_SHOWMAXIMIZED = 3;
        const int SW_SHOW = 5;

        [Flags]
        public enum ThreadAccess : int {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

    

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern int SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        public static void HideWindow(this Process Process) {
            if (!HandlerMap.ContainsKey(Process.Id))
                HandlerMap[Process.Id] = new List<IntPtr>();

            IntPtr Handler;
            if ((Handler = Process.MainWindowHandle) != IntPtr.Zero) {
                HandlerMap[Process.Id].Add(Handler);
                ShowWindow(Handler, SW_HIDE);
            }
        }
        public static void ShowWindow(this Process Process) {
            if (!HandlerMap.ContainsKey(Process.Id))
                return;

            foreach (IntPtr Handle in HandlerMap[Process.Id])
                ShowWindow(Handle, SW_SHOW);
        }

        public static void FocusMainWindow(this Process Process, bool Maximized = true) {
            IntPtr Handler = Process.MainWindowHandle;
            if (Handler == IntPtr.Zero)
                return;

            ShowWindow(Handler, Maximized ? SW_SHOWMAXIMIZED : SW_SHOWNORMAL);
            SetForegroundWindow(Handler);
        }
        public static void SuspendProcess(this Process Process) {
            if (Process.ProcessName == string.Empty)
                return;

            if (!ThreadMap.ContainsKey(Process.Id))
                ThreadMap[Process.Id] = new List<ProcessThread>();

            foreach (ProcessThread pT in Process.Threads) {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero) {
                    continue;
                }

                bool IsSuspended = pT.ThreadState == ThreadState.Wait && pT.WaitReason == ThreadWaitReason.Suspended;
                
                if (!IsSuspended) {
                    SuspendThread(pOpenThread);

                    if (!ThreadMap[Process.Id].Contains(pT))
                        ThreadMap[Process.Id].Add(pT);
                }

                CloseHandle(pOpenThread);
            }
        }

        public static void ResumeProcess(this Process Process) {
            if (Process.ProcessName == string.Empty)
                return;

            ProcessThread[] Threads;
            if (ThreadMap.ContainsKey(Process.Id))
                Threads = ThreadMap[Process.Id].ToArray();
            else
                Threads = Process.Threads.Cast<ProcessThread>().ToArray();

            foreach (ProcessThread pT in Threads) {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero) {
                    continue;
                }

                var suspendCount = 0;
                do {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);

                if (ThreadMap.ContainsKey(Process.Id))
                    if (ThreadMap[Process.Id].Contains(pT))
                        ThreadMap[Process.Id].Remove(pT);
            }
        }

        public static void MuteProcess(this Process Process, bool Mute) {
            AudioController.AudioManager.SetApplicationMute(Process.Id, Mute);
        }
    }
}
