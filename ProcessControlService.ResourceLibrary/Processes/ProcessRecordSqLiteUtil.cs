// ==================================================
// 文件名：ProcessRecordSqLiteUtil.cs
// 创建时间：2020/06/16 17:03
// ==================================================
// 最后修改于：2020/07/15 17:03
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using log4net;
using ProcessControlService.Contracts.ProcessData;
using ProcessControlService.ResourceFactory.DBUtil;

namespace ProcessControlService.ResourceLibrary.Processes
{
    /// <summary>
    ///     Process历史记录SQLite工具包 sunjian added 2020 4月
    /// </summary>
    public class ProcessRecordSqLiteUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessRecordSqLiteUtil));

        private static readonly object ThreadLocker = new object();

        /// <summary>
        ///     定期清理数据库数据，间隔时间为0.5天
        /// </summary>
        private static readonly Timer Timer = new Timer(0.5 /*天*/ * 24 /*小时*/ * 60 /*分钟*/ * 60 /*秒*/ * 1000 /*毫秒*/);

        /// <summary>
        ///     记录Process历史记录
        /// </summary>
        /// <param name="processInstanceRecord"></param>
        public static void LogProcessInstance(ProcessInstanceRecord processInstanceRecord)
        {
            try
            {
                var recordDirectoryInfo = new DirectoryInfo(FreeSqlUtil.BaseDirectory);

                if (!recordDirectoryInfo.Exists) recordDirectoryInfo.Create();
                
                lock (ThreadLocker)
                {
                    var baseRepository = FreeSqlUtil.FSql.GetRepository<ProcessInstanceRecord>();
                    
                    baseRepository.DbContextOptions.EnableAddOrUpdateNavigateList = true;

                    baseRepository.Insert(processInstanceRecord);
                }
            }
            catch (Exception e)
            {
                Log.Error(
                    $"记录完成的过程实例数据失败，记录的Process为[{processInstanceRecord.ProcessName}],异常为:[{e.Message}，{e.StackTrace}]\n {e.InnerException}.");
            }
        }

        /// <summary>
        ///     读取Process历史记录
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="pageSize"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="searchPage"></param>
        /// <returns></returns>
        public static List<ProcessInstanceRecord> ReadProcessRecord(string processName,
            int pageSize, DateTime startDate, DateTime endDate, int searchPage)
        {
            var processInstanceRecords = new List<ProcessInstanceRecord>();

            try
            {
                lock (ThreadLocker)
                {
                    processInstanceRecords = FreeSqlUtil.FSql.Select<ProcessInstanceRecord>().Where(a => a.ProcessName == processName)
                        .Where(a => a.StartTime >= startDate && a.StartTime<endDate).IncludeMany(a=>a.Messages)
                        .IncludeMany(a=>a.Parameters)
                        .OrderByDescending(a => a.StartTime).Page(searchPage,pageSize).ToList();

                    return processInstanceRecords;
                }
            }
            catch (Exception e)
            {
                Log.Error($"获取过程实例历史数据失败，获取的Process为[{processName}],异常为:[{e.Message}].");

                return processInstanceRecords;
            }
        }

        /// <summary>
        ///     启动Process历史记录，定时清理计时器
        /// </summary>
        public static void Work()
        {
            Timer.Elapsed += RemoveProcessRecord;
            Timer.Start();
        }
        
        /// <summary>
        ///     清理5天前的Process历史记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsedEventArgs"></param>
        private static void RemoveProcessRecord(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                lock (ThreadLocker)
                {
                    var recordDirectoryInfo = new DirectoryInfo(FreeSqlUtil.BaseDirectory);
                    if (!recordDirectoryInfo.Exists) recordDirectoryInfo.Create();

                    var checkDate = DateTime.Now.AddDays(-10);
                    var memoryCheckDate = DateTime.Now.AddDays(-30);

                    FreeSqlUtil.FSql.Delete<ProcessInstanceRecord>().Where(a => a.StartTime <= checkDate).ExecuteAffrows();
                    FreeSqlUtil.FSql.Delete<MemoryAndCpuData>().Where(a => a.RecordDate <= memoryCheckDate).ExecuteAffrows();
                }
            }
            catch (Exception e)
            {
                Log.Error($"移除过程实例历史数据失败，异常为:[{e.Message}].");
            }
        }
    }
}