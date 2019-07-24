using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace videoplay
{
    class ScreenCapture
    {
        /// <summary>
        /// 创建一个Image对象，其中包含整个桌面的屏幕截图
        /// </summary>
        /// <returns></returns>
        public Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }
        /// <summary>
        /// 创建一个包含特定窗口的屏幕截图的Image对象
        /// </summary>
        /// <param name="handle">窗口的句柄。（在Windows窗体中，这是通过Handle属性获得的）</param>
        /// <returns></returns>
        public Image CaptureWindow(IntPtr handle)
        {
            // 获得目标窗口的hDC
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // 获得尺寸
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // 创建我们可以复制到的设备上下文
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // 创建一个我们可以复制到的位图，
            // 使用GetDeviceCaps获取宽度/高度
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // 选择位图对象
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt结束
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // 恢复选择
            GDI32.SelectObject(hdcDest, hOld);
            // 清空
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // 获取它的.NET图像对象
            Image img = Image.FromHbitmap(hBitmap);
            // 释放Bitmap对象
            GDI32.DeleteObject(hBitmap);
            return img;
        }
        /// <summary>
        /// 捕获特定窗口的屏幕截图，并将其保存到文件中
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }

        /// <summary>
        /// 包含Gdi32 API函数的Helper类
        /// </summary>
        private class GDI32
        {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop参数
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary>
        /// 包含User32 API函数的Helper类
        /// </summary>
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        }
    }
}
