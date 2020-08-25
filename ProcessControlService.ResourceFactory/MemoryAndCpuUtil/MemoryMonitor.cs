// ==================================================
// 文件名：MemoryMonitor.cs
// 创建时间：2020/06/01 15:04
// ==================================================
// 最后修改于：2020/06/11 15:04
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using ProcessControlService.Contracts.ProcessData;
using ProcessControlService.ResourceFactory.DBUtil;
using Timer = System.Timers.Timer;

namespace ProcessControlService.ResourceFactory.MemoryAndCpuUtil
{
    /// <summary>
    ///     系统内存和CPU使用率监控器
    /// </summary>
    /// <remarks>
    ///  add by sunjian 2020-05
    /// 
    ///     每个10min点记录一次内存使用大小和CPU占用率，
    ///     该10min点程序没有启用，记0
    ///     每小时6个数据，每天6*24=144个点
    /// </remarks>
    public static class MemoryMonitor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MemoryMonitor));

        private static readonly object ThreadLocker = new object();
        
        const int KbDiv = 1024;
        const int MbDiv = 1024 * 1024;

        private static bool _init = true;

        public static bool IsTestMode = false;

        private static readonly Timer Timer = new Timer(1000 * 60 * 10 /*10min*/);
        private static PerformanceCounter _cpuCounter;
        private static string _processName;
        private static PerformanceCounter _ramCounter;

        public static void Start()
        {
            Timer.Elapsed += RecordMemoryAndCpuUsage;
            
            _processName = Process.GetCurrentProcess().ProcessName;

            _cpuCounter = new PerformanceCounter("Process", "% Processor Time", _processName);
            _ramCounter = new PerformanceCounter("Process", "Working Set - Private", _processName);
            
            _cpuCounter.NextValue();
            _ramCounter.NextValue();
            
            Task.Run(() =>
            {
                while (_init)
                {
                    //初始化计数时间点，10分钟正点计数
                    if (DateTime.Now.Minute % 10 == 0)
                    {
                        _init = false;
                        RecordMemoryAndCpuUsage(null, null);
                        Timer.Start();
                        Log.Info($"CPU和内存捕捉计时器已启动");
                        break;
                    }

                    Thread.Sleep(5000);
                }
            });
        }

        /// <summary>
        ///     记录内存和Cpu占用率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void RecordMemoryAndCpuUsage(object sender, ElapsedEventArgs e)
        {
            try
            {
                var memoryAndCpuData = new MemoryAndCpuData
                {
                    RecordTimeIndex = GetCurrentTimeIndex(),
                    Memory = GetProcessRam(),
                    CpuUsage = GetCpuUsage(),
                    RecordDate = DateTime.Today
                };

                //检查或创建ProcessRecord文件夹
                lock (ThreadLocker)
                {
                    var recordDirectoryInfo = new DirectoryInfo(FreeSqlUtil.BaseDirectory);

                    if (!recordDirectoryInfo.Exists) 
                        recordDirectoryInfo.Create();

                    Log.Info($"进程Cpu占用率：[{memoryAndCpuData.CpuUsage}]%,内存占用:[{memoryAndCpuData.Memory}]MB");

                    try
                    {
                        FreeSqlUtil.FSql.Insert(memoryAndCpuData).ExecuteAffrows();
                    }
                    catch (Exception exception)
                    {
                        Thread.Sleep(5000);
                        
                        FreeSqlUtil.FSql.Insert(memoryAndCpuData).ExecuteAffrows();
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("记录程序使用内存和CPU占用率失败，异常为：" + $"{exception}");
            }
        }

        /// <summary>
        ///     获取内存和cpu使用率日志
        /// </summary>
        /// <param name="searchDate"></param>
        /// <returns></returns>
        public static List<MemoryAndCpuData> GetMemoryAndCpuData(DateTime searchDate)
        {
            var memoryAndCpuData = new List<MemoryAndCpuData>();

            try
            {
                lock (ThreadLocker)
                {
                    memoryAndCpuData = FreeSqlUtil.FSql.Select<MemoryAndCpuData>()
                        .Where(a => a.RecordDate== searchDate).ToList();

                    return memoryAndCpuData;
                }
            }
            catch (Exception e)
            {
                Log.Error("记录程序使用内存和CPU占用率失败，异常为：" + $"{e}");

                return memoryAndCpuData;
            }
        }
        
        /// <summary>
        ///     获取当前时间节点Index
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>
        ///     每10分钟为一个序列记录点
        ///     如0:00 为0
        ///     0:10 为1
        ///     0:20 为2
        ///     一次类推
        /// </remarks>
        private static int GetCurrentTimeIndex()
        {
            var now = DateTime.Now;
            var minuteAggregate = now.Hour * 60 + now.Minute;

            return IsTestMode ? minuteAggregate * 60 + now.Second : minuteAggregate / 10;
        }

        /// <summary>
        ///     获取进程使用内存大小
        /// </summary>
        /// <returns></returns>
        private static double GetProcessRam()
        {
            return Math.Round(_ramCounter.NextValue() / MbDiv, 2);
        }

        /// <summary>
        ///     获取Cpu占用率
        /// </summary>
        /// <returns></returns>
        private static double GetCpuUsage()
        {
            return Math.Round(_cpuCounter.NextValue() / Environment.ProcessorCount, 2);
        }
        
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            var ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
    }
}