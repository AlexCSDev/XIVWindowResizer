using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace XIVWindowResizer.Helpers
{
    //Fully managed implementation of the window resizing logic from https://github.com/ProjectMimer/xivr
    public unsafe class WindowSizeHelper(WindowSearchHelper windowSearchHelper)
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool AdjustWindowRect(ref Rect lpRect, uint dwStyle, bool bMenu);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int SendMessageW(IntPtr hWnd, int wMsg, int wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
            ;
        private IntPtr gameWindowHandle = IntPtr.Zero;

        public void SetWindowSize(int width, int height)
        {
            if (gameWindowHandle == IntPtr.Zero)
            {
                gameWindowHandle = windowSearchHelper.FindWindowHandle();
                if (gameWindowHandle == IntPtr.Zero)
                    throw new Exception("Unable to find window handle");
            }

            //Tell the game to update resolution
            var dev = Device.Instance();
            dev->NewWidth = (uint)width;
            dev->NewHeight = (uint)height;
            dev->RequestResolutionChange = 1;

            //Resize window
            var clientRect = new Rect(0, 0, width, height);

            var cx = clientRect.Right - clientRect.Left;
            var cy = clientRect.Bottom - clientRect.Top;

            AdjustWindowRect(ref clientRect, (uint)GetWindowLongPtr(gameWindowHandle, (int)GWL.GWL_STYLE), false);
            SetWindowPos(gameWindowHandle, 0, 0, 0, cx, cy, (uint)(SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_FRAMECHANGED));
            SendMessageW(gameWindowHandle, 0x0232, 0, 0); //WM_EXITSIZEMOVE
        }

        public static Size GetWindowSize()
        {
            var dev = Device.Instance();
            return new Size((int)dev->Width, (int)dev->Height);
        }
    }

    //The following code was taken from http://pinvoke.net
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect(int left, int top, int right, int bottom)
    {
        public int Left = left, Top = top, Right = right, Bottom = bottom;

        public Rect(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

        public int X
        {
            get => Left;
            set { Right -= Left - value; Left = value; }
        }

        public int Y
        {
            get => Top;
            set { Bottom -= Top - value; Top = value; }
        }

        public int Height
        {
            get => Bottom - Top;
            set => Bottom = value + Top;
        }

        public int Width
        {
            get => Right - Left;
            set => Right = value + Left;
        }

        public Point Location
        {
            get => new(Left, Top);
            set { X = value.X; Y = value.Y; }
        }

        public Size Size
        {
            get => new(Width, Height);
            set { Width = value.Width; Height = value.Height; }
        }

        public static implicit operator Rectangle(Rect r)
        {
            return new Rectangle(r.Left, r.Top, r.Width, r.Height);
        }

        public static implicit operator Rect(Rectangle r)
        {
            return new Rect(r);
        }

        public static bool operator ==(Rect r1, Rect r2)
        {
            return r1.Equals(r2);
        }

        public static bool operator !=(Rect r1, Rect r2)
        {
            return !r1.Equals(r2);
        }

        public bool Equals(Rect r)
        {
            return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
        }

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                Rect rect => Equals(rect),
                Rectangle rectangle => Equals(new Rect(rectangle)),
                _ => false
            };
        }

        public override int GetHashCode()
        {
            return ((Rectangle)this).GetHashCode();
        }

        public override string ToString() => string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
    }
}
