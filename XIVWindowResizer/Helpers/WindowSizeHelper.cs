using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using XIVWindowResizer.Enums;

namespace XIVWindowResizer.Helpers;

//Fully managed implementation of the window resizing logic from https://github.com/ProjectMimer/xivr
public unsafe class WindowSizeHelper
{
    private readonly WindowSearchHelper _windowSearchHelper;

    [DllImport("user32.dll")]
    static extern bool AdjustWindowRect(ref Rect lpRect, uint dwStyle, bool bMenu);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, UInt32 uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SendMessageA(IntPtr hWnd, int wMsg, int wParam, int lParam);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    private IntPtr _gameWindowHandle = IntPtr.Zero;

    public WindowSizeHelper(WindowSearchHelper windowSearchHelper)
    {
        _windowSearchHelper = windowSearchHelper;
    }

    public void SetWindowSize(int width, int height)
    {
        if (_gameWindowHandle == IntPtr.Zero)
        {
            _gameWindowHandle = _windowSearchHelper.FindWindowHandle();
            if (_gameWindowHandle == IntPtr.Zero)
                throw new Exception("Unable to find window handle");
        }

        //Tell the game to update resolution
        Device* dev = Device.Instance();
        dev->NewWidth = (uint)width;
        dev->NewHeight = (uint)height;
        dev->RequestResolutionChange = 1;

        //Resize window
        Rect clientRect = new Rect(0, 0, width, height);

        int cx = clientRect.Right - clientRect.Left;
        int cy = clientRect.Bottom - clientRect.Top;

        AdjustWindowRect(ref clientRect, (uint)GetWindowLongPtr(_gameWindowHandle, (int)GWL.GWL_STYLE), false);
        SetWindowPos(_gameWindowHandle, 0, 0, 0, cx, cy, (uint)(SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_FRAMECHANGED));
        SendMessageA(_gameWindowHandle, 0x0232, 0, 0); //WM_EXITSIZEMOVE
    }

    public Size GetWindowSize()
    {
        Device* dev = Device.Instance();
        return new Size((int)dev->Width, (int)dev->Height);
    }
}

//The following code was taken from http://pinvoke.net
[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
    public int Left, Top, Right, Bottom;

    public Rect(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public Rect(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

    public int X
    {
        get { return Left; }
        set { Right -= (Left - value); Left = value; }
    }

    public int Y
    {
        get { return Top; }
        set { Bottom -= (Top - value); Top = value; }
    }

    public int Height
    {
        get { return Bottom - Top; }
        set { Bottom = value + Top; }
    }

    public int Width
    {
        get { return Right - Left; }
        set { Right = value + Left; }
    }

    public System.Drawing.Point Location
    {
        get { return new System.Drawing.Point(Left, Top); }
        set { X = value.X; Y = value.Y; }
    }

    public System.Drawing.Size Size
    {
        get { return new System.Drawing.Size(Width, Height); }
        set { Width = value.Width; Height = value.Height; }
    }

    public static implicit operator System.Drawing.Rectangle(Rect r)
    {
        return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
    }

    public static implicit operator Rect(System.Drawing.Rectangle r)
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

    public override bool Equals(object obj)
    {
        if (obj is Rect)
            return Equals((Rect)obj);
        else if (obj is System.Drawing.Rectangle)
            return Equals(new Rect((System.Drawing.Rectangle)obj));
        return false;
    }

    public override int GetHashCode()
    {
        return ((System.Drawing.Rectangle)this).GetHashCode();
    }

    public override string ToString()
    {
        return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
    }
}
