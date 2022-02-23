using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Dashy.Utils
{
    public class WindowMenuHelper
    {
        private readonly IntPtr _windowHandle;
        private readonly List<(string Label, Action Callback)> _callbacks;

        public WindowMenuHelper(Window window)
        {
            _windowHandle = new WindowInteropHelper(window).Handle;
            _callbacks = new List<(string Label, Action Callback)>();
            var hwndSource = HwndSource.FromHwnd(_windowHandle);
            hwndSource?.AddHook(OnWindowMenuClick);
        }

        public void InsertSeparator()
        {
            var systemMenuHandle = GetSystemMenu(_windowHandle, false);
            InsertMenu(systemMenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
        }

        public void InsertMenuItem(string label, Action callback)
        {
            _callbacks.Add((label, callback));
            var systemMenuHandle = GetSystemMenu(_windowHandle, false);
            InsertMenu(systemMenuHandle, 5 + _callbacks.Count, MF_BYPOSITION, 1000 - 1 + _callbacks.Count, label);
        }

        private IntPtr OnWindowMenuClick(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SYSCOMMAND)
            {
                var id = wParam.ToInt32();

                if (id >= 1000 && id - 1000 < _callbacks.Count)
                {
                    _callbacks[id - 1000].Callback();
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, int wPosition, int wFlags, int wIDNewItem, string lpNewItem);

        //A window receives this message when the user chooses a command from the Window menu, or when the user chooses the maximize button, minimize button, restore button, or close button.
        private const int WM_SYSCOMMAND = 0x112;

        //Draws a horizontal dividing line.This flag is used only in a drop-down menu, submenu, or shortcut menu.The line cannot be grayed, disabled, or highlighted.
        private const int MF_SEPARATOR = 0x800;

        //Specifies that an ID is a position index into the menu and not a command ID.
        private const int MF_BYPOSITION = 0x400;

        //Specifies that the menu item is a text string.
        private const int MF_STRING = 0x0;
    }
}