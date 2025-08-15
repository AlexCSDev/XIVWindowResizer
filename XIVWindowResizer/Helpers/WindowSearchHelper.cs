using System;
using System.Runtime.InteropServices;

namespace XIVWindowResizer.Helpers
{
    public class WindowSearchHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lClassName, string windowTitle);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        public IntPtr FindWindowHandle()
        {
            IntPtr handle = IntPtr.Zero;
            while ((handle = FindWindowEx(IntPtr.Zero, handle, "FFXIVGAME", "FINAL FANTASY XIV")) != IntPtr.Zero)
            {
                _ = GetWindowThreadProcessId(handle, out var pid);

                if (pid == Environment.ProcessId && IsWindowVisible(handle))
                    break;
            }

            return handle;
        }
    }
}
