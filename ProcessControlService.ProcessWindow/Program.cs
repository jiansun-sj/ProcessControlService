// ==================================================
// 文件名：Program.cs
// 创建时间：2020/05/02 13:36
// ==================================================
// 最后修改于：2020/05/25 13:36
// 修改人：jians
// ==================================================

using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.Services;
using System;
using System.ServiceModel;
using System.ServiceProcess;
using WebSocketSharp.Server;

namespace ProcessControlService.ProcessWindow
{
    internal static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        private static WebSocketServer _wssv1;

        private static ServiceHost _machineServiceHost;
        private static ServiceHost _processServiceHost;
        private static ServiceHost _resourceServiceHost;
        private static ServiceHost _partnerServiceHost;
        private static ServiceHost _adminServiceHost;

        /// <summary>
        ///     应用程序的主入口点。
        /// </summary>
        private static void Main(string[] args)
        {
            //Console.Title = "ProcessWindow服务程序";
            Console.Title = @"ProcessWindow服务程序";
            ConsoleWin32Helper.SetTitle(Console.Title);
            // 控制台运行
            //    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            //Log.Info("上海芸浦信息技术有限公司 版权所有2018");
            Log.Info("copyright@yumpoo2018");
            Log.Info(
                "说明 \n Tag的UpdateInterval的基数为毫秒 \n WaitAction的WaitTime基数是毫秒 \n Step的Wait的基数是秒 \n TimerCondition的Seconds是0.1秒 \n Action的默认超时是5秒");

            OpenWCFPorts();

            //if (args.Length > 0)
            //{
            //    var par = args[0].ToLower();
            //    if (par == "c" || par == "/c" || par == "-c" || par == "console") 
            RunAsConsole();
            //}
            //else
            //{
            //    RunAsService();
            //}


        }

        private static void RunAsConsole()
        {
            StartProcessControlService();

            Log.Info("程序启动...");

#if Release
            ConsoleWin32Helper.ShowFlag = true;
            ConsoleWin32Helper.ShowNotifyIcon();
            ConsoleWin32Helper.ShowHideWindow();
            ConsoleWin32Helper.DisableCloseBtn();
            while (!ConsoleWin32Helper.ExitCall)
            {
                Thread.Sleep(100);
                Application.DoEvents();
            }
#endif
            //Console.ReadLine();
        }


        private static void OpenWebSocketPorts()
        {
            try
            {
                _wssv1 = new WebSocketServer(12669);
                _wssv1.AddWebSocketService<WsServiceProxy>("/WSService");
                _wssv1.Start();
                Log.Info($"WebSocket端口{_wssv1.Port}已开启");
            }
            catch (Exception e)
            {
                _wssv1 = null;
                Log.Error($"WebSocket端口打开失败：{e.Message}");
            }
        }

        public static void StartProcessControlService(bool runInService = false)
        {
            try
            {
                Log.Info(runInService ? "ProcessWindows以服务方式运行" : "ProcessWindows以控制台方式运行");

                SystemManager.Start(runInService);

                //var files = Directory.GetFiles(xmlPath, "*.xml");

                //ResourceManager.StartAllResource();
            }
            catch (Exception ex)
            {
                Log.Error($"装载ProcessWindow出错:{ex}");
            }
        }

        /*        private static bool _isWebSocketOpen;

                private static void ChangeWebSocketConnection(object sender, Redundancy.ChangeModeArgs newMode)
                {
                    if (newMode.RedundancyMode==RedundancyMode.Master)
                    {
                        if (_isWebSocketOpen) return;
                        OpenWebSocketPorts();
                        _isWebSocketOpen = true;
                    }

                    else
                    {
                        ConnectionManager.CloseWebSocketConnection();
                    }

                }*/
        //static public void StartProcessServiceAsService()
        //{
        //    try
        //    {
        //        ResourceManager.InitResourceAsService();
        //        //string appPath = Directory.GetCurrentDirectory();
        //        //string appPath = "C:\\Debug\\service";             //路径
        //        string appPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        //        string xmlPath = appPath + "Config";

        //        var files = Directory.GetFiles(xmlPath, "*.xml");

        //        // 载入配置文件
        //        foreach (var file in files)
        //        {
        //            LOG.Info(string.Format("载入配置文件:{0}", file));
        //            ResourceManager.LoadConfig(file);

        //        }
        //        ResourceManager.StartAllResource();

        //        // 打开端口连接
        //        OpenWCFPorts();
        //    }
        //    catch (Message ex)
        //    {
        //        LOG.UnKnown(string.Format("装载ProcessWindow出错:{0}", ex));
        //    }

        //}

        private static void RunAsService()
        {
            //LOG.Info(string.Format("程序服务方式启动..."));
            //LOG.Info(string.Format("程序集版本{0}",
            //    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            //LOG.Info(string.Format("上海芸浦信息技术有限公司 版权所有2018"));
            //LOG.Info(string.Format("说明 \n Tag的UpdateInterval的基数为毫秒 \n WaitAction的WaitTime基数是毫秒 \n Step的Wait的基数是秒 \n TimerCondition的Seconds是0.1秒 \n Action的默认超时是5秒"));
            //System.Threading.Thread.Sleep(30000); //延迟启动 调试用

            // 服务运行
            var servicesToRun = new ServiceBase[]
            {
                new ProcessWindowService()
            };

            ServiceBase.Run(servicesToRun);
        }

        private static ServiceHost ResourceServiceHost = null;
        private static void OpenWCFPorts()
        {
            //打开WCF服务端口

            #region MachineService host

            var machineService = new MachineService();
            try
            {
                _machineServiceHost = new ServiceHost(machineService);
                _machineServiceHost.Opened += delegate
                {
                    Log.Info($"机器服务已成功启动:{_machineServiceHost.BaseAddresses[0]}MachineService.");
                };

                _machineServiceHost.Open();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                _machineServiceHost = null;
            }

            #endregion

            #region ProcessService host

            var processService = new ProcessService();
            try
            {
                _processServiceHost = new ServiceHost(processService);
                _processServiceHost.Opened += delegate
                {
                    Log.Info($"过程服务已成功启动:{_processServiceHost.BaseAddresses[0]}ProcessService.");
                };

                _processServiceHost.Open();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                _processServiceHost = null;
            }

            #endregion

            #region ResourceService host

            try
            {
                _resourceServiceHost = new ServiceHost(typeof(ResourceService));
                _resourceServiceHost.Opened += delegate
                {
                    Log.Info($"资源服务已成功启动:{_resourceServiceHost.BaseAddresses[0]}ResourceService.");
                };

                _resourceServiceHost.Open();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                _resourceServiceHost = null;
            }

            //WcfService.HttpFactory.CreateServers(
            //   new List<Type> { typeof(ResourceService) },
            //   (t) => { return t.Name; },
            //   (t) => { return typeof(IResourceService); },
            //   "WcfServices",
            //   8789,
            //   (sender, exception) => { Log.Info(exception); },
            //   (msg) => { Log.Info(msg); },
            //   (msg) => { Log.Info(msg); },
            //   (msg) => { Log.Info(msg); });

            #endregion

            #region PartnerService host

            var partnerService = new PartnerService();
            try
            {
                _partnerServiceHost = new ServiceHost(partnerService);
                _partnerServiceHost.Opened += delegate
                {
                    Log.Info($"冗余检测服务已成功启动:{_partnerServiceHost.BaseAddresses[0]}PartnerService.");
                };

                _partnerServiceHost.Open();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                _partnerServiceHost = null;
            }

            #endregion

            #region AdminService host

            var adminService = new AdminService();
            try
            {
                _adminServiceHost = new ServiceHost(adminService);
                _adminServiceHost.Opened += delegate
                {
                    Log.Info($"系统管理服务已成功启动:{_adminServiceHost.BaseAddresses[0]}AdminService.");
                };

                _adminServiceHost.Open();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                _adminServiceHost = null;
            }

            #endregion

            #region WebService host

            var webService = new ResourceWebService();
            try
            {
                var serviceHost = new ServiceHost(typeof(ResourceWebService));
                serviceHost.Opened += delegate
                {
                    Log.Info($"WebService服务已成功启动:{_adminServiceHost.BaseAddresses[0]}WebService.");
                };

                serviceHost.Open();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            #endregion

            OpenWebSocketPorts();
        }

        public static void Close()
        {
            //if (_machineServiceHost != null)
            //{
            //    _machineServiceHost.Close();
            //    _machineServiceHost = null;
            //}

            //if (_processServiceHost != null)
            //{
            //    _processServiceHost.Close();
            //    _processServiceHost = null;
            //}

            if (_resourceServiceHost != null)
            {
                _resourceServiceHost.Close();
                _resourceServiceHost = null;
            }

            //if (_partnerServiceHost != null)
            //{
            //    _partnerServiceHost.Close();
            //    _partnerServiceHost = null;
            //}

            //if (_adminServiceHost != null)
            //{
            //    _adminServiceHost.Close();
            //    _adminServiceHost = null;
            //}
            //ResourceManager.FreeAllResources();
        }
    }
}