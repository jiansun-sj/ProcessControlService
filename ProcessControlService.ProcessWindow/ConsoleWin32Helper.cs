// ==================================================
// 文件名：ConsoleWin32Helper.cs
// 创建时间：2020/07/31 13:28
// ==================================================
// 最后修改于：2020/07/31 13:28
// 修改人：jians
// ==================================================

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ProcessControlService.ResourceFactory;

namespace ProcessControlService.ProcessWindow
{
    /// <summary>
    ///     系统图标操作
    /// </summary>
    internal class ConsoleWin32Helper
    {
        public static bool ShowFlag = true;
        public static bool ExitCall;

        static ConsoleWin32Helper()
        {
            _NotifyIcon.Icon = new Icon(@"./favicon.ico");
            _NotifyIcon.Visible = false;
            _NotifyIcon.Text = Title;

            var menu = new ContextMenu();
            _NotifyIcon.ContextMenu = menu;


            var show = new MenuItem
            {
                Text = "打开窗口",
                Index = 0
            };
            show.Click += ShowClick;
            menu.MenuItems.Add(show);

            var hide = new MenuItem
            {
                Text = "隐藏窗口",
                Index = 1
            };
            hide.Click += HideClick;
            menu.MenuItems.Add(hide);


            //MenuItem load = new MenuItem
            //{
            //    Text = "重新启动",
            //    Index = 2
            //};
            //load.Click += new EventHandler(RestartClick);
            //menu.MenuItems.Add(load);


            var exit = new MenuItem
            {
                Text = "退出",
                Index = 2
            };
            exit.Click += ExitClick;
            menu.MenuItems.Add(exit);

            _NotifyIcon.MouseDoubleClick += _NotifyIcon_MouseDoubleClick;
        }

        public static string Title { get; set; } = "";

        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private static void ExitClick(object sender, EventArgs e)
        {
            var res = MessageBox.Show($"是否要关闭{Title}?", "", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
            {
                ExitCall = true;
                ResourceManager.GetAllResources();
                Environment.Exit(0);
            }
        }

        private static void ShowClick(object sender, EventArgs e)
        {
            var intptr = FindWindow(null, Title);
            if (intptr != IntPtr.Zero) ShowWindow(intptr, 1); //显示　1
        }

        private static void HideClick(object sender, EventArgs e)
        {
            var intptr = FindWindow(null, Title);
            if (intptr != IntPtr.Zero) ShowWindow(intptr, 0); //隐藏　0
        }

        private static void _NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowHideWindow();
        }

        public static void SetTitle(string p_strTitle)
        {
            Title = p_strTitle;
            _NotifyIcon.Text = $"{Title}，双击显示或隐藏窗口";
        }

        public static void ShowHideWindow()
        {
            var intptr = FindWindow(null, Title);
            if (intptr != IntPtr.Zero)
            {
                if (ShowFlag)
                    ShowWindow(intptr, 0); //隐藏　0
                else
                    ShowWindow(intptr, 1); //显示　1
            }

            ShowFlag = !ShowFlag;
        }

        #region 托盘图标

        private static readonly NotifyIcon _NotifyIcon = new NotifyIcon();

        public static void ShowNotifyIcon()
        {
            _NotifyIcon.Visible = true;
            _NotifyIcon.ShowBalloonTip(10000, "", $"{Title}", ToolTipIcon.None);
        }

        public static void HideNotifyIcon()
        {
            _NotifyIcon.Visible = false;
        }

        public static void DisableCloseBtn()
        {
            //根据控制台标题找控制台
            var WINDOW_HANDLER = FindWindow(null, Title);
            //找关闭按钮
            var CLOSE_MENU = GetSystemMenu(WINDOW_HANDLER, IntPtr.Zero);
            var SC_CLOSE = 0xF060;
            //关闭按钮禁用
            RemoveMenu(CLOSE_MENU, SC_CLOSE, 0x0);
        }


        [DllImport("user32.dll ", EntryPoint = "GetSystemMenu")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

        [DllImport("user32.dll ", EntryPoint = "RemoveMenu")]
        private static extern int RemoveMenu(IntPtr hMenu, int nPos, int flags);

        #endregion
    }
}