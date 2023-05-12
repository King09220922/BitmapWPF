using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bitmap.Properties
{
    internal class AllWindowsIntPtr
    {
        public static Dictionary<string, IntPtr> intPtrDictionary = new Dictionary<string, IntPtr>();

        // 导入Windows API函数
        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


        private static bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            int length = GetWindowTextLength(hWnd);
            if (length > 0)
            {
                StringBuilder stringBuilder = new StringBuilder(length + 1);
                GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);

                string title = stringBuilder.ToString();
                if (!string.IsNullOrEmpty(title) && IsWindowVisible(hWnd))
                {
                    Console.WriteLine("标题: " + title);
                    intPtrDictionary[title] = hWnd;
                }
            }
            return true;
        }

        public static void GetEnumWindows()
        {
            intPtrDictionary.Clear();
            // 枚举所有窗口句柄
            EnumWindows(EnumWindowsCallback, IntPtr.Zero);
        }
    }
}
