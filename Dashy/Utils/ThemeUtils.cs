using System;
using System.Runtime.InteropServices;

namespace Dashy.Utils
{
    public static class ThemeUtils
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

        public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (!IsWindows10OrGreater(17763))
            {
                return false;
            }

            var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;

            if (IsWindows10OrGreater(18985))
            {
                attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
            }

            var useImmersiveDarkMode = enabled ? 1 : 0;
            return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

        public static bool SetWindowCornerPreference(IntPtr handle, bool round)
        {
            if (!IsWindows10OrGreater(17763))
            {
                return false;
            }

            var attribute = DWMWA_WINDOW_CORNER_PREFERENCE;
            var preference = round ? 0 : 1;
            return DwmSetWindowAttribute(handle, attribute, ref preference, sizeof(int)) == 0;
        }
    }
}