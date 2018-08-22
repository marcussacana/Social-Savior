//Original Source: https://github.com/yuzhengyang/WindowsStartup/blob/master/WindowsStartup/WindowsStartup/Utils/ShortcutTool.cs
using System.IO;

namespace WindowsStartup.Utils {
    public class ShortcutTool {
        public static string Create(string directory, string shortcutName, string targetPath, string arguments = null, string description = null, string iconLocation = null) {
            try {
                string shortcutPath = GetShortcutPath(directory, shortcutName);
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.Arguments = arguments;
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcut.WindowStyle = 1;
                shortcut.Description = description;
                shortcut.IconLocation = string.IsNullOrWhiteSpace(iconLocation) ? targetPath : iconLocation;
                shortcut.Save();

                return shortcutPath;
            } catch { }
            return null;
        }

        public static string GetShortcutPath(string directory, string shortcutName) {
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            return Path.Combine(directory, string.Format("{0}.lnk", shortcutName));
        }

        public static bool Delete(string directory, string shortcutName) {
            try {
                string shortcutPath = Path.Combine(directory, string.Format("{0}.lnk", shortcutName));
                if (File.Exists(shortcutPath)) {
                    File.Delete(shortcutPath);
                }
                return true;
            } catch { }
            return false;
        }
    }
}