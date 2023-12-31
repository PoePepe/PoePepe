﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PoePepe.UI.Helpers;

public static class WindowsInternalFeatureService
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);


    // Activate an application window.
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr  hWnd, StringBuilder title, int size);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32")]
    public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32")]
    public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32")]
    public static extern IntPtr WndProc(IntPtr hwnd, int code, IntPtr wParam, IntPtr lParam);


    [DllImport("user32")]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    // Needed to prevent Application taking focus
    [DllImport("user32")]
    public static extern bool LockSetForegroundWindow(uint UINT);

    public static IntPtr FindPoeGameWindow() => FindWindow("POEWindowClass", "Path of Exile");

    public static bool SetForegroundPoeGameWindow()
    {
        var ptr = FindWindow("POEWindowClass", "Path of Exile");
        return SetForegroundWindow(ptr);
    }

    public static string GetWindowTitle(IntPtr hWnd)
    {
        var length = GetWindowTextLength(hWnd) + 1;
        var title = new StringBuilder(length);
        GetWindowText(hWnd, title, length);
        return title.ToString();
    }
}