using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace videoplay
{
    /// <summary>
    /// 定义全屏抽象类
    /// </summary>
    public abstract class FullScreenObject
    {
        public abstract void FullScreen(bool flag);
    }
    /// <summary>
    /// 桌面全屏
    /// </summary>
    public sealed class FullScreenHelper: FullScreenObject
    {
        bool m_bFullScreen = false;

        private static volatile FullScreenHelper _fsh = null;
 
        Control m_control = null;

        private FullScreenHelper(Control control)
        {
            m_control = control;
        }

        public static FullScreenHelper createInstance(Control control)
        {
            if (_fsh == null)
            {
                _fsh = new FullScreenHelper(control);
            }
            return _fsh;            
        }
 
        private IntPtr m_OldWndParent = IntPtr.Zero;
 
        DockStyle old_docker_style;
        int old_left;
        int old_width;
        int old_height;
        int old_top;
 
        public override void FullScreen(bool flag)
        {
            m_bFullScreen = flag;
            if (!m_bFullScreen)
            {
                // 取消全屏设置
                m_control.Dock = old_docker_style;
                m_control.Left = old_left;
                m_control.Top = old_top;
                m_control.Width = old_width;
                m_control.Height = old_height;
                ShellSDK.SetParent(m_control.Handle, m_OldWndParent);
            }
            else
            {
                // 记录原来的数据
                old_docker_style = m_control.Dock;
                old_left = m_control.Left;
                old_width = m_control.Width;
                old_height = m_control.Height;
                old_top = m_control.Top;
                m_OldWndParent = ShellSDK.GetParent(m_control.Handle);
                // 设置全屏数据
                int nScreenWidth = ShellSDK.GetSystemMetrics(0);
                int nScreenHeight = ShellSDK.GetSystemMetrics(1);
                m_control.Dock = DockStyle.None;
                m_control.Left = 0;
                m_control.Top = 0;
                m_control.Width = nScreenWidth;
                m_control.Height = nScreenHeight;
                ShellSDK.SetParent(m_control.Handle, ShellSDK.GetDesktopWindow());
                ShellSDK.SetWindowPos(m_control.Handle, -1, 0, 0, m_control.Right - m_control.Left, m_control.Bottom - m_control.Top, 0);
            }
            m_bFullScreen = !m_bFullScreen;
        }
    }
 
    /// <summary>
    /// Windows系统API-SDK
    /// </summary>
    public class ShellSDK
    {
        //函数来设置弹出式窗口，层叠窗口或子窗口的父窗口。新的窗口与窗口必须属于同一应用程序
        [DllImport("User32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
 
        //该函数返回桌面窗口的句柄。桌面窗口覆盖整个屏幕。桌面窗口是一个要在其上绘制所有的图标和其他窗口的区域
        [DllImport("User32.dll")]
        public static extern IntPtr GetDesktopWindow();
 
        //是用于得到被定义的系统数据或者系统配置信息的一个专有名词  
        [DllImport("User32.dll")]
        public static extern int GetSystemMetrics(int nIndex); 
 
        [DllImport("user32.dll", EntryPoint = "GetParent", SetLastError = true)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);
    }
}
