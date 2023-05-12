
namespace Bitmap.Properties
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public class DwmapiWrapper
    {

        private const int SRCCOPY = 0x00CC0020;
        private static readonly Dictionary<IntPtr, IntPtr> _windowHdcCache = new Dictionary<IntPtr, IntPtr>();

        #region DllImport DLL
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        // 导入Dwmapi.dll库中的DwmExtendFrameIntoClientArea函数
        [DllImport("Dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        // 导入Dwmapi.dll库中的DwmIsCompositionEnabled函数
        [DllImport("Dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool pfEnabled);

        // 导入Dwmapi.dll库中的DwmEnableBlurBehindWindow函数
        [DllImport("Dwmapi.dll")]
        private static extern int DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);

        // 导入User32.dll库中的GetWindowRect函数
        [DllImport("User32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // 导入Gdi32.dll库中的CreateDC函数
        [DllImport("Gdi32.dll")]
        private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        // 导入Gdi32.dll库中的CreateCompatibleBitmap函数
        [DllImport("Gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        // 导入Gdi32.dll库中的CreateCompatibleDC函数
        [DllImport("Gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        // 导入Gdi32.dll库中的SelectObject函数
        [DllImport("Gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        // 导入Gdi32.dll库中的BitBlt函数
        [DllImport("Gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        // 导入Gdi32.dll库中的DeleteDC函数
        [DllImport("Gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);
        #endregion

        // 定义MARGINS结构体，这个结构体是DwmExtendFrameIntoClientArea函数的参数之一
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
            public MARGINS(int left, int right, int top, int bottom)
            {
                cxLeftWidth = left;
                cxRightWidth = right;
                cyTopHeight = top;
                cyBottomHeight = bottom;
            }
        }

        // 定义DWM_BLURBEHIND结构体，这个结构体是DwmEnableBlurBehindWindow函数的参数之一
        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_BLURBEHIND
        {
            public uint dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
            public const uint DWM_BB_ENABLE = 0x00000001;
            public const uint DWM_BB_BLURREGION = 0x00000002;
            public const uint DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004;
        }

        // 定义RECT结构体，这个结构体是GetWindowRect函数的参数之一
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// 捕捉当前屏幕主屏幕画面
        /// </summary>
        /// <returns></returns>
        public static Bitmap CaptureScreen()
        {
            // 获取桌面窗口句柄
            IntPtr desktopWindow = GetDesktopWindow();
            // 获取桌面窗口矩形区域
            GetWindowRect(desktopWindow, out RECT rect);
            // 计算桌面窗口的宽度和高度
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            // 创建设备上下文，用于绘制桌面的内容
            IntPtr hdcSrc = CreateDC("DISPLAY", null, null, IntPtr.Zero);
            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            SelectObject(hdcDest, hBitmap);
            // 绘制桌面的内容到设备上下文中
            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, rect.Left, rect.Top, SRCCOPY);
            // 将设备上下文中的内容保存到位图中
            Bitmap bitmap = Image.FromHbitmap(hBitmap);
            // 释放资源
            DeleteDC(hdcSrc);
            DeleteDC(hdcDest);
            DeleteObject(hBitmap);
            // 返回位图
            return bitmap;
        }
        public static Bitmap CaptureWindowInBackgroundC(IntPtr hwnd)
        {
            if (!IsWindow(hwnd))
            {
                throw new ArgumentException("Invalid window handle");
            }

            RECT windowRect;
            if (!GetWindowRect(hwnd, out windowRect))
            {
                throw new Win32Exception();
            }

            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;

            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr hdcDest = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;
            try
            {
                hdcSrc = GetWindowDC(hwnd);
                if (hdcSrc == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                hdcDest = CreateCompatibleDC(hdcSrc);
                if (hdcDest == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
                if (hBitmap == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                IntPtr hOld = SelectObject(hdcDest, hBitmap);
                if (hOld == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, SRCCOPY);
                SelectObject(hdcDest, hOld);
                Bitmap bitmap = Image.FromHbitmap(hBitmap);
                if (bitmap == null)
                {
                    throw new Win32Exception();
                }

                return bitmap;
            }
            finally
            {
                if (hdcSrc != IntPtr.Zero)
                {
                    ReleaseDC(hwnd, hdcSrc);
                }

                if (hdcDest != IntPtr.Zero)
                {
                    DeleteDC(hdcDest);
                }

                if (hBitmap != IntPtr.Zero)
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        /// <summary>
        /// 后台截取屏幕画面
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static Bitmap CaptureWindowInBackgroundA(IntPtr hwnd)
        {
            GetWindowRect(hwnd, out RECT windowRect);
            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;
            IntPtr hdcSrc = GetWindowDC(hwnd);
            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOld = SelectObject(hdcDest, hBitmap);
            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, SRCCOPY);
            SelectObject(hdcDest, hOld);
            ReleaseDC(hwnd, hdcSrc);
            Bitmap bitmap = Image.FromHbitmap(hBitmap);
            // 释放资源
            DeleteDC(hdcSrc);
            DeleteDC(hdcDest);
            DeleteObject(hBitmap);
            // 返回位图
            return bitmap;
        }

        /// <summary>
        /// 后台截取屏幕画面
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static Bitmap CaptureWindowInBackgroundB(IntPtr hwnd)
        {
            GetWindowRect(hwnd, out RECT windowRect);
            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;
            IntPtr hdcSrc = GetWindowDC(hwnd);
            IntPtr hdcDest = CreateCompatibleDC(IntPtr.Zero); // Pass IntPtr.Zero to create the DC for the entire screen
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOld = SelectObject(hdcDest, hBitmap);
            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, SRCCOPY);
            SelectObject(hdcDest, hOld);
            DeleteDC(hdcDest);
            ReleaseDC(hwnd, hdcSrc);
            Bitmap result = Bitmap.FromHbitmap(hBitmap);
            DeleteObject(hBitmap);
            return result;
        }
    }

}
