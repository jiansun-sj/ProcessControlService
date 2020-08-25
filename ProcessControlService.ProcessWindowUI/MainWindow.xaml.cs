using System;
using System.Windows;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;

namespace ProcessControlService.ProcessWindowUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        public MainWindow()
        {
            InitializeComponent();
            this.Title = "FactoryWindowUI " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.BalloonTipText = "FactoryWindowUI... ...";
            this.notifyIcon.ShowBalloonTip(2000);
            this.notifyIcon.Text = "FactoryWindowUI... ...";
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            this.notifyIcon.Visible = true;
            //打开菜单项
            MenuItem open = new MenuItem("Open");
            open.Click += new EventHandler(Show);
            //退出菜单项
            MenuItem exit = new MenuItem("Exit");
            exit.Click += new EventHandler(Close);
            //关联托盘控件
            MenuItem[] childen = new MenuItem[] { open, exit };
            notifyIcon.ContextMenu = new ContextMenu(childen);

            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) Show(null, null);
            });

            this.Closing += (s, r) =>
            {
                r.Cancel = true;
                Hide(null, null);

            };
        }

        private void Show(object sender, EventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                this.Visibility = Visibility.Visible;
                this.Activate();
            }
        }

        private void Hide(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Close(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Button_Install(object sender, RoutedEventArgs e)
        {
            try
            {                
                string targetDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "service";
                Process proc = new Process();
                proc.StartInfo.WorkingDirectory = targetDir;
                proc.StartInfo.FileName = "Install.bat";
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.WaitForExit();

                Thread.Sleep(1000);

                if (proc.HasExited == false)
                {
                    proc.Kill();
                }
                proc.Close();
                System.Windows.MessageBox.Show("安装服务成功");

                
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("安装服务失败{0}", ex));  
            }
        }

        private void Button_Uninstall(object sender, RoutedEventArgs e)
        {
            try
            {
                string targetDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "service";
                Process proc = new Process();
                proc.StartInfo.WorkingDirectory = targetDir;
                proc.StartInfo.FileName = "Uninstall.bat";
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.WaitForExit();

                Thread.Sleep(1000);

                if (proc.HasExited == false)
                {
                    proc.Kill();
                }
                proc.Close();
                System.Windows.MessageBox.Show("卸载服务成功");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("卸载服务失败{0}", ex));
            }
           
        }

        private void Button_Start(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceController serviceController = new ServiceController("ProcessWindow");
                if(serviceController.Status== ServiceControllerStatus.Stopped)
                {
                    serviceController.Start();
                    System.Windows.MessageBox.Show("启动服务成功");
                }
                else
                {
                    System.Windows.MessageBox.Show("服务不在停止状态");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("启动服务失败{0}", ex));
            }
         
        }

        private void Button_Stop(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceController serviceController = new ServiceController("ProcessWindow");
                if (serviceController.CanStop && serviceController.Status == ServiceControllerStatus.Running)
                {
                    serviceController.Stop();
                    System.Windows.MessageBox.Show("停止服务成功");
                }
                else
                {
                    System.Windows.MessageBox.Show("服务不在运行状态");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("停止服务失败{0}", ex));
            }
            
        }

        private void Button_Pause(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("抱歉~该功能暂不可用");  
            //try
            //{
            //    ServiceController serviceController = new ServiceController("ProcessWindow");
            //    if (serviceController.CanPauseAndContinue)
            //    {
            //        if (serviceController.Status == ServiceControllerStatus.Running)
            //            serviceController.Pause();
            //        else if (serviceController.Status == ServiceControllerStatus.Paused)
            //            serviceController.Continue();
            //    }

            //    MessageBox.Show("暂停/恢复服务成功");
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(string.Format("暂停/恢复服务失败{0}", ex));
            //}
           
        }

        private void Button_CheckStatus(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceController serviceController = new ServiceController("ProcessWindow");
                string Status = serviceController.Status.ToString();
                Label_ServiceStatus.Text = Status;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("错误{0}", ex.Message ));
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Hide(null, null);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("版本："+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
    }
}
