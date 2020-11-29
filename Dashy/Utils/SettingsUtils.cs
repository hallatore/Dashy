using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Dashy.Utils
{
    public static class SettingsUtils
    {        
        public static string GetSettingsPath()
        {
            var file = new OpenFileDialog
            {
                FileName = "Select a settings.json file",
                Filter = "Settings files (*.json)|*.json",
                Title = "Open settings file",
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (file.ShowDialog() == true)
            {
                var fileName = file.FileName;
                fileName = fileName.Replace(Directory.GetCurrentDirectory() + "\\", "");

                if (fileName.StartsWith("configs\\"))
                {
                    fileName = fileName.Substring("configs\\".Length);
                }

                if (fileName.EndsWith("settings.json"))
                {
                    fileName = Path.GetDirectoryName(fileName);
                }

                return fileName;
            }

            return null;
        }

        public static void CreateShortcut(string path)
        {
            var shortcutName = path;

            if (shortcutName.EndsWith(".json"))
            {
                shortcutName = Path.GetDirectoryName(shortcutName);
            }

            if (shortcutName.EndsWith("\\"))
            {
                shortcutName = shortcutName.Substring(0, shortcutName.Length - 1);
            }

            shortcutName = Path.GetFileName(shortcutName);
            var lnkPath = Path.Combine(Directory.GetCurrentDirectory(), $"{shortcutName}.lnk");
            var exePath = Path.Combine(Directory.GetCurrentDirectory(), "Dashy.exe");

            if (File.Exists(lnkPath))
            {
                MessageBox.Show($"Shortcut \"{shortcutName}\" already exists. Use this to start the app directly.");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell", 
                Arguments = $"$shortcut=(New-Object -COM WScript.Shell).CreateShortcut('{lnkPath}');$shortcut.TargetPath='{exePath}';$shortcut.WorkingDirectory='{Directory.GetCurrentDirectory()}';$shortcut.Arguments='{path}';$shortcut.Save();",
                UseShellExecute = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            });

            MessageBox.Show($"Shortcut \"{shortcutName}\" was created. Use this to start this app directly in the future.");
        }
    }
}
