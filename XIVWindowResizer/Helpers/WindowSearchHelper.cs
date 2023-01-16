using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XIVWindowResizer.Helpers
{
    public class WindowSearchHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        public IntPtr FindWindowHandle()
        {
            IntPtr handle = IntPtr.Zero;
            while ((handle = FindWindowEx(IntPtr.Zero, handle, "FFXIVGAME", "FINAL FANTASY XIV")) != IntPtr.Zero)
            {
                _ = GetWindowThreadProcessId(handle, out int pid);

                if (pid == Environment.ProcessId && IsWindowVisible(handle))
                    break;
            }

            return handle;
        }
    }
}
