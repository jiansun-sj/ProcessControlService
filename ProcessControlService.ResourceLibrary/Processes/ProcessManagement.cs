// ==================================================
// 文件名：ProcessManagement.cs
// 创建时间：2020/01/02 13:49
// ==================================================
// 最后修改于：2020/05/21 13:49
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Common;

namespace ProcessControlService.ResourceLibrary.Processes
{
    public class ProcessManagement
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessManagement));

        public static readonly ProcessInstanceManager ProcessInstanceManager = new ProcessInstanceManager();

        private static bool _isWorking;

        public static int GetProcessInstancesNumber(string processName)
        {
            return ProcessInstanceManager.GetProcessInstancesNumber(processName);
        }

        public static IEnumerable<ProcessInstance> GetProcessInstances(string processName)
        {
            return ProcessInstanceManager.GetProcessInstances(processName);
        }

        public static void StartCheckCondition()
        {
            if (_isWorking) return;
            _isWorking = true;

            var checkThread = new Thread(CheckConditionThread);
            checkThread.Start();

            ProcessRecordSqLiteUtil.Work();
        }

        //停止Process运行 David 20190807
        public static void StopCheckCondition()
        {
            _isWorking = false;
        }

        private static void CheckConditions()
        {
            // step 1: Check all conditions 
            foreach (var resource in ResourceManager.GetTypedResources("Process"))
            {
                var process = (Process) resource;

                // 如果处于手动模式则跳过
                if (!process.AutoMode)
                    continue;

                if (process.CheckCondition())
                {
                    if (process.LogOutput) //0521Robin加
                        Log.Info($"过程 {process.ResourceName} 被触发.");

                    //在ProcessInstance复制Process参数之前，需要锁定ProcessParameter，不能让MultiTagCondition更改对应的Process参数
                    //防止高并发情况下，下一个触发Process的MultiTagCondition的OutMachine覆盖了上一次触发生成的对应的Process参数。
                    //sunjian 2020-07-21 长春
                    if (!process.AllowAsync)
                    {
                        if (ProcessInstanceManager.RunInstance(process.ProcessName))
                        {
                            Log.Warn($"{process.ProcessName}有正在运行的Process实例，不允许重复执行。");
                            continue;
                        }
                    }

                    var initializeProcessInstance = process.InitializeProcessInstance();

                    ThreadPool.QueueUserWorkItem((o)=>
                    {
                        process.CreateProcessInstance(initializeProcessInstance);
                    }, process);
                }
            }
        }

        private static void CheckConditionThread()
        {
            while (_isWorking)
            {
                
                CheckConditions(); 
                
                Thread.Sleep(100); //基础时间是1秒   Robin5月21日改为100(0.1秒)
            }
        }

        public static void RunProcessInstance(ProcessInstance processInstance)
        {
            //判断Process是否允许重入。
            if (!processInstance.AllowAsync && ProcessInstanceManager.RunInstance(processInstance.ProcessName))
            {
                Log.Info($"过程：{processInstance.ProcessName}正在运行不允许重复运行");
                return;
            }

            ProcessInstanceManager.ProcessInstances.Add(processInstance);
            
            processInstance.Execute();
        }

        public static void RemoveFinishedProcessInstance(ProcessInstance processInstance)
        {
            processInstance.StopWork();
            ProcessInstanceManager.RemoveProcessInstance(processInstance.Pid);
        }

        public static void CallProcessActionRunInstance(Process process, ResourceDicModel<string> resourceDicModel)
        {
            if (process.AllowAsync)
                Task.Run(() =>
                {
                    var initializeProcessInstance = process.InitializeProcessInstance(resourceDicModel);
                    process.CreateProcessInstance(initializeProcessInstance,resourceDicModel);
                });
            else
            {
                var initializeProcessInstance = process.InitializeProcessInstance(resourceDicModel);
                process.CreateProcessInstance(initializeProcessInstance,resourceDicModel);
            }
        }

        public static string GeneratePid()
        {
            try
            {
                var pid = "-1";

                //防止极其特殊情况下创建了重复的GUID，可以重新生成两次，三次生成都重复
                for (var i = 0; i < 3; i++)
                {
                    pid = Guid.NewGuid().ToString("N");

                    if (ProcessInstanceManager.ProcessInstances.All(a => a.Pid != pid))
                        break;
                    //创建了重复pid，pid复位-1
                    Log.Error($"ProcessInstanceManager第{i + 1}次创建过程实例参数Id重复，Pid复位，开始重新创建Pid。");
                    pid = "-1";
                }

                return pid;
            }
            catch (Exception e)
            {
                Log.Error($"ProcessInstanceManager创建过程实例参数Id失败，{e.Message}");
                return "-1";
            }
        }
    }
}