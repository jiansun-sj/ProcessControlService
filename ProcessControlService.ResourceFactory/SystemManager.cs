// ==================================================
// 文件名：SystemManager.cs
// 创建时间：2020/05/02 13:43
// ==================================================
// 最后修改于：2020/05/25 13:43
// 修改人：jians
// ==================================================

using System;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Xml;
using log4net;
using Microsoft.Win32;
using ProcessControlService.ResourceFactory.DBUtil;
using ProcessControlService.ResourceFactory.MemoryAndCpuUtil;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceFactory.RegisterControl;
using Timer = System.Timers.Timer;

namespace ProcessControlService.ResourceFactory
{
    public static class SystemManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResourceManager));

        public static void Start(bool serviceMode = false)
        {
            //检查授权
            var bCheckSuccess = serviceMode ? CheckRegAsService() : CheckReg();
            if (!bCheckSuccess)
            {
                Log.Error("检查授权失败!5秒后程序开始退出!");
                Thread.Sleep(5000);
                Environment.Exit(0);
            }

            // 设置线程数
            ThreadPool.SetMinThreads(20, 20);
            ThreadPool.SetMaxThreads(200, 200);
            Log.Info("最小线程数20，最大200");

            //初始化资源
            ResourceManager.StartAllResource();

            //启动系统内存和CPU监控
            MemoryMonitor.Start();
        }

        public static void Stop()
        {

        }

        private static bool CheckReg()
        {
            var success = false;
            try
            {
                var reg = new CheckReg();
                if (!reg.GetIsReg())
                {
                    var machineNum = reg.GetMachineNum();
                    Console.WriteLine("程序还未注册，请将以下机器码发送给程序作者获取注册码");
                    Console.WriteLine("机器码：" + machineNum);
                    var k = GetCommand();
                    switch (k)
                    {
                        case 'Q':
                        case 'q':
                            Log.Debug("5秒后程序退出!");
                            Thread.Sleep(5000);
                            Environment.Exit(0);
                            break;

                        case 'b':
                        case 'B':
                            success = JumpOverRegister();
                            break;

                        case 'a':
                        case 'A':
                        case 'y':
                        case 'Y':
                            {
                                Console.WriteLine("请输入注册码:");
                                while (true)
                                {
                                    var regNum = Console.ReadLine()?.Trim();
                                    if (!reg.IsRegnumOK(regNum))
                                    {
                                        if (regNum == "7777777")
                                        {
                                            success = TemporaryAuthorize();
                                            break;
                                        }

                                        Console.WriteLine("您输入的注册码不正确");
                                        /*Console.WriteLine("Q-----退出");
                                        Console.WriteLine("B-----跳过注册");
                                        Console.WriteLine("C-----重新输入");*/
                                        k = GetCommand();
                                        if (k == 'Q' || k == 'q')
                                        {
                                            Log.Debug("3秒后程序退出!");
                                            Thread.Sleep(3000);
                                            Environment.Exit(0);
                                        }
                                        else if (k == 'b' || k == 'B')
                                        {
                                            success = JumpOverRegister();
                                            break;
                                        }
                                        else if (k == 'Y' || k == 'y' || k == 'a' || k == 'A')
                                        {
                                            Console.WriteLine("请输入注册码:");
                                        }
                                        else
                                        {
                                            Console.WriteLine("指令不识别,请输入注册码:");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("注册成功");
                                        var retKey = Registry.CurrentUser.OpenSubKey("Software", true)
                                            ?.CreateSubKey("Hosting")
                                            ?.CreateSubKey("Register.INI")
                                            ?.CreateSubKey(regNum ?? string.Empty);
                                        retKey?.SetValue("UserName", "Rsoft");
                                        //StartProcessService();
                                        success = true;
                                        break;
                                    }
                                }

                                break;
                            }
                        default:
                            Log.Debug("指令不正确,5秒后程序退出!");
                            Thread.Sleep(5000);
                            Environment.Exit(0);
                            break;
                    }
                }
                else
                {
                    //StartProcessService();
                    success = true;
                    Log.Info("程序授权验证已通过!");
                }
            }
            catch (Exception ex)
            {
                success = false;
                Log.Error("检测授权出现异常出错:" + ex.Message);
            }

            return success;
        }

        private static char GetCommand()
        {
            Console.WriteLine("请输入指令");
            Console.WriteLine("Y-----开始注册");
            Console.WriteLine("Q-----退出");
            Console.WriteLine("A-----临时授权");
            Console.WriteLine("B-----跳过");
            var k = Console.ReadKey(false).KeyChar;
            return k;
        }

        /// <summary>
        /// 跳过授权，试用2小时。
        /// </summary>
        /// <returns></returns>
        private static bool JumpOverRegister()
        {
            var t = new Timer(7200000);

            t.Elapsed += CloseSys; //到达时间的时候执行事件；
            t.AutoReset = true; //设置是执行一次（false）还是一直执行(true)；
            t.Enabled = true; //是否执行System.Timers.Timer.Elapsed事件；

            Log.Warn($"\n程序未授权，试用2小时，请联系程序作者获取永久使用权");
            Log.Warn("程序启动了计时器2小时");

            //StartProcessService();
            return true;
        }

        /// <summary>
        /// 临时授权90天。
        /// </summary>
        /// <returns></returns>
        private static bool TemporaryAuthorize()
        {
            var retKey = Registry.CurrentUser.OpenSubKey("Software", true)
                ?.OpenSubKey("Hosting")
                ?.OpenSubKey("ExpireDate.INI");

            if (retKey == null)
            {
                Registry.CurrentUser.OpenSubKey("Software", true)
                    ?.CreateSubKey("Hosting")
                    ?.CreateSubKey("ExpireDate.INI")
                    ?.SetValue("ExpireDate", "");

                retKey = Registry.CurrentUser.OpenSubKey("Software", true)
                    ?.OpenSubKey("Hosting")
                    ?.OpenSubKey("ExpireDate.INI");
            }

            if (retKey != null)
            {
                var expireDate = retKey.GetValue("ExpireDate").ToString();

                if (string.IsNullOrEmpty(expireDate))
                {
                    expireDate = DateTime.Now.AddDays(90).ToString("G");
                    Registry.CurrentUser.OpenSubKey("Software", true)
                        ?.CreateSubKey("Hosting")
                        ?.CreateSubKey("ExpireDate.INI")
                        ?.SetValue("ExpireDate", expireDate);
                }

                if (DateTime.Now >= Convert.ToDateTime(expireDate))
                {
                    Log.Warn($"\n程序使用期限截止日期为{expireDate}，请联系程序作者获取永久使用权");
                    CloseSys(null, null);
                }

                Log.Warn($"\n程序使用期限截止日期为{expireDate}，请联系程序作者获取永久使用权");
            }

            return true;
        }
        
        private static bool CheckRegAsService()
        {
            bool success;
            try
            {
                var reg = new CheckReg();
                if (!reg.GetIsReg())
                {
                    Log.Warn("程序没有授权,以试用模式运行！若需注册授权,请以命令行模式启动程序！");
                   
                    //if (!reg.GetUseInfo())
                    //{
                    //     t = new System.Timers.Timer(7200000);
                    //}
                    //else
                    //{
                    //   t = new System.Timers.Timer(-DateTime.Now.Subtract(Convert.ToDateTime("2018-01-15 00:00:00")).TotalMilliseconds*1000);
                    //}
                    /*var t = new Timer(7200000);
                    //t = new System.Timers.Timer(60000);

                    t.Elapsed += CloseSys; //到达时间的时候执行事件；
                    t.AutoReset = true; //设置是执行一次（false）还是一直执行(true)；
                    t.Enabled = true; //是否执行System.Timers.Timer.Elapsed事件；
                    Log.Debug("程序启动了计时器2小时");*/

                    //StartProcessService();

                    success = JumpOverRegister();
                }
                else
                {
                    //StartProcessService();
                    success = true;
                    Log.Info("程序授权验证已通过!");
                }
            }
            catch (Exception ex)
            {
                success = false;
                Log.Error("检测授权出现异常出错:" + ex.Message);
            }

            return success;
        }

        private static void CloseSys(object source, ElapsedEventArgs e)
        {
            Log.Warn("程序授权时间已到,5秒后程序开始退出!");
            Thread.Sleep(5000);
            Environment.Exit(0);
        }

        private static bool CheckRegStatus(bool serviceMode)
        {
            //检查授权
            return serviceMode ? CheckRegAsService() : CheckReg();
        }
        
        // 装载入口
        public static void LoadConfig(string configFile)
        {
            try
            {
                // loading from xml file
                var doc = new XmlDocument();
                doc.Load(configFile);

                // load  configuration
                var configs = doc.SelectSingleNode("root/Configs");
                if (null != configs)
                    foreach (var item in configs.Cast<XmlNode>().Where(item => item.NodeType != XmlNodeType.Comment))
                        if (item.Name == "Redundancy") // Get redundancy config -- Dongmin 20180319
                        {
                            var redundancy = (XmlElement)item;
                            if (redundancy.HasAttribute("Enable"))
                            {
                                var strEnableRedundancy = redundancy.GetAttribute("Enable");
                                ResourceManager.EnableRedundancy = strEnableRedundancy.ToLower() == "true";
                            }
                        }
                        else if (item.Name == "DBConnection")
                        {
                            var dbConfig = (XmlElement)item;
                            if (dbConfig.HasAttribute("Name") && dbConfig.HasAttribute("Type") &&
                                dbConfig.HasAttribute("ConnectionString"))
                            {
                                var name = dbConfig.GetAttribute("Name");
                                var type = DataBaseHelper.ConvertDatabaseType(dbConfig.GetAttribute("Type"));

                                if (DataBaseHelper.NameType.Keys.Contains(name)) continue;
                                var connectionStr = dbConfig.GetAttribute("ConnectionString");
                                DataBaseHelper.NameType.Add(name, type);
                                DataBaseHelper.NameConnStr.Add(name, connectionStr);
                            }

                            else
                                throw new Exception("数据库配置错误,缺少Name,Type或ConnectionString");
                        }
                        else
                            throw new Exception("该节点不识别:" + item.Name);

                // Load Custom values
                var customValues = doc.SelectSingleNode("root/CustomTypes");
                if (null != customValues)
                    foreach (XmlNode item in customValues)
                    {
                        if (item.NodeType == XmlNodeType.Comment) continue;

                        // 装载自定义类型 -- Dongmin 20180602
                        // 装载所有实现了ICustomType接口的类型 -- 孙健 2020-07
                        CustomTypeManager.LoadCustomTypesInConfig(item);
                    }

                // Load Resources
                var resources = doc.SelectSingleNode("root/Resources");
                if (resources != null)
                    foreach (XmlNode item in resources)
                    {
                        if (item.NodeType == XmlNodeType.Comment) continue;

                        ResourceManager.LoadResourceInConfig(item, configFile, doc);
                    }
            }
            catch (Exception ex)
            {
                Log.Error($"装载配置文件{configFile}失败{ex.Message}");
            }
        }
    }
}