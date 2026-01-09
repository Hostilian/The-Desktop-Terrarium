using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Terrarium.Desktop
{
    public partial class MainWindow
    {
        private void RegisterGlobalHotkeys(IntPtr hwnd)
        {
            RegisterHotKey(hwnd, HotkeySave, ModControl | ModAlt | ModNoRepeat,
                (uint)KeyInterop.VirtualKeyFromKey(Key.S));

            RegisterHotKey(hwnd, HotkeyLoad, ModControl | ModAlt | ModNoRepeat,
                (uint)KeyInterop.VirtualKeyFromKey(Key.L));

            RegisterHotKey(hwnd, HotkeyToggleStatus, ModControl | ModAlt | ModNoRepeat,
                (uint)KeyInterop.VirtualKeyFromKey(Key.F1));
        }

        private void UnregisterGlobalHotkeys(IntPtr hwnd)
        {
            UnregisterHotKey(hwnd, HotkeySave);
            UnregisterHotKey(hwnd, HotkeyLoad);
            UnregisterHotKey(hwnd, HotkeyToggleStatus);
        }

        private static void ApplyNoActivateStyles(IntPtr hwnd)
        {
            IntPtr exStylePtr = GetWindowLongPtr(hwnd, GwlExStyle);
            long exStyle = exStylePtr.ToInt64();

            exStyle |= WsExNoActivate;
            exStyle |= WsExToolWindow;

            SetWindowLongPtr(hwnd, GwlExStyle, new IntPtr(exStyle));
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WmHotKey)
            {
                int hotkeyId = wParam.ToInt32();
                switch (hotkeyId)
                {
                    case HotkeySave:
                        SaveGame();
                        handled = true;
                        return IntPtr.Zero;

                    case HotkeyLoad:
                        LoadGame();
                        handled = true;
                        return IntPtr.Zero;

                    case HotkeyToggleStatus:
                        ToggleUIVisibility();
                        handled = true;
                        return IntPtr.Zero;
                }
            }

            if (msg != WmNcHitTest)
            {
                return IntPtr.Zero;
            }

            // If we haven't started, don't block input.
            if (_simulationEngine == null)
            {
                handled = true;
                return new IntPtr(HtTransparent);
            }

            // Screen coords packed into lParam.
            int x = GetSignedLowWord(lParam);
            int y = GetSignedHighWord(lParam);

            Point screenPoint = new Point(x, y);
            Point windowPoint = PointFromScreen(screenPoint);
            Point canvasPoint = TranslateToCanvasPoint(windowPoint);

            var clickable = _simulationEngine.FindClickableAt(canvasPoint.X, canvasPoint.Y);
            if (clickable == null)
            {
                handled = true;
                return new IntPtr(HtTransparent);
            }

            return IntPtr.Zero;
        }

        private static int GetSignedLowWord(IntPtr ptr)
        {
            int value = unchecked((short)((long)ptr & 0xFFFF));
            return value;
        }

        private static int GetSignedHighWord(IntPtr ptr)
        {
            int value = unchecked((short)(((long)ptr >> 16) & 0xFFFF));
            return value;
        }

        private Point TranslateToCanvasPoint(Point windowPoint)
        {
            // RenderCanvas is inside the window; translate the point.
            return RenderCanvas.TransformToAncestor(this).Inverse.Transform(windowPoint);
        }
    }
}
