// ==================================================
// 文件名：ProcessRecordXmlUtil.cs
// 创建时间：2020/03/16 9:47
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/03/17 9:47
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using log4net;
using ProcessControlService.Contracts.ProcessData;

namespace ProcessControlService.ResourceLibrary.Processes
{
    public class ProcessRecordXmlUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessRecordXmlUtil));

        private static readonly object ThreadLocker = new object();
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory + "ProcessRecord";

        private static string SerializeObj<T>(T t)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, t);

            return stringWriter.ToString();
        }

        private static T DeserializeObj<T>(string xml)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var stringReader = new StringReader(xml);
            var obj = xmlSerializer.Deserialize(stringReader);
            stringReader.Close();
            return (T) obj;
        }

        public static void LogProcessInstance(ProcessInstanceRecord processInstanceRecord)
        {
            try
            {
                lock (ThreadLocker)
                {
                    //var processInstanceRecords = ReadProcessRecord("TaskOneMainProcess", 10);

                    var fileInfos = ProcessRecordFileInfos();

                    string processLogFileName;

                    //目录下没有任何的xml日志，则创建xml日志
                    if (!fileInfos.Any())
                    {
                        processLogFileName = BaseDirectory + "\\ProcessLog~01.xml";
                        WriteToFile(processLogFileName, processInstanceRecord, FileMode.Create);

                        return;
                    }

                    //目录下存在xml，将Process执行日志记录到最后修改的文件中。
                    if (fileInfos[0].Length < 1024 * 1024 * 10 /*10M文本大小*/)
                    {
                        processLogFileName = fileInfos[0].FullName;

                        WriteToFile(processLogFileName, processInstanceRecord, FileMode.Append);

                        return;
                    }

                    var substring = fileInfos[0].Name.Substring("ProcessLog~".Length, 2);
                    int.TryParse(substring, out var logIndex);

                    //如果文档大于10M则向下一个文档中记录数据
                    if (logIndex < 30)
                    {
                        logIndex++;
                        var logIndexString = logIndex >= 10 ? logIndex.ToString() : "0" + logIndex;
                        processLogFileName = BaseDirectory + $"\\ProcessLog~{logIndexString}.xml";

                        //如果不存在该文件，则创建一个新的文档
                        WriteToFile(processLogFileName, processInstanceRecord, FileMode.Create);
                        return;
                    }

                    //达到日志记录数量上限，从头开始记录
                    processLogFileName = BaseDirectory + "\\ProcessLog~01.xml";
                    WriteToFile(processLogFileName, processInstanceRecord, FileMode.Create);
                }
            }
            catch (Exception e)
            {
                Log.Error($"记录完成的过程实例数据失败，记录的Process为[{processInstanceRecord.ProcessName}],异常为:[{e.Message}].");
            }
        }

        private static List<FileInfo> ProcessRecordFileInfos()
        {
            var recordDirectoryInfo = new DirectoryInfo(BaseDirectory);

            if (!recordDirectoryInfo.Exists) recordDirectoryInfo.Create();

            //获取Process日志下所有Xml，按修改日期倒序
            var fileInfos = recordDirectoryInfo.GetFiles("*.xml").OrderByDescending(a => a.LastWriteTime).ToList();
            return fileInfos;
        }

        private static void WriteToFile(string processLogFileName, ProcessInstanceRecord processRecord,
            FileMode fileMode)
        {
            var doc = new XmlDocument();

            switch (fileMode)
            {
                case FileMode.CreateNew:
                    break;
                case FileMode.Create:
                    //  创建XML文档，存在就删除再生成

                    var dec = doc.CreateXmlDeclaration("1.0", "GB2312", null);
                    doc.AppendChild(dec);
                    //  创建根结点
                    var root = doc.CreateElement("root");
                    doc.AppendChild(root);

                    AddProcessRecord(doc);

                    break;
                case FileMode.Open:
                    break;
                case FileMode.OpenOrCreate:
                    break;
                case FileMode.Truncate:
                    break;
                case FileMode.Append:

                    doc.Load(processLogFileName);
                    AddProcessRecord(doc);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileMode), fileMode, null);
            }

            doc.Save(processLogFileName);

            void AddProcessRecord(XmlDocument xmlDoc)
            {
                var root = xmlDoc.SelectSingleNode("root");

                var processInstanceRecord = xmlDoc.CreateElement("ProcessInstanceRecord");
                processInstanceRecord.SetAttribute("ProcessName", processRecord.ProcessName);
                processInstanceRecord.SetAttribute("Pid", processRecord.Pid);
                processInstanceRecord.SetAttribute("StartTime", processRecord.StartTime.ToString(CultureInfo.InvariantCulture));
                processInstanceRecord.SetAttribute("EndTime", processRecord.EndTime.ToString(CultureInfo.InvariantCulture));
                processInstanceRecord.SetAttribute("ProcessStatus", processRecord.ProcessStatus.ToString());
                processInstanceRecord.SetAttribute("BreakStepId", processRecord.BreakStepId.ToString());
                processInstanceRecord.SetAttribute("BreakStepName", processRecord.BreakStepName);

                var exceptions = xmlDoc.CreateElement("Exceptions");

                foreach (var exception in processRecord.Messages)
                {
                    var exceptionNode = xmlDoc.CreateElement("string");
                    exceptionNode.InnerText = exception.Description;
                    exceptions.AppendChild(exceptionNode);
                }

                var parameters = xmlDoc.CreateElement("Parameters");

                foreach (var parameterInfo in processRecord.Parameters)
                {
                    var parameterNode = xmlDoc.CreateElement("ParameterInfo");
                    parameterNode.SetAttribute("Name", parameterInfo.Name);
                    parameterNode.SetAttribute("ValueInString", parameterInfo.ValueInString);
                    parameterNode.SetAttribute("Type", parameterInfo.Type);
                    parameterNode.SetAttribute("Key", parameterInfo.Key);

                    parameters.AppendChild(parameterNode);
                }

                processInstanceRecord.AppendChild(exceptions);
                processInstanceRecord.AppendChild(parameters);

                root?.AppendChild(processInstanceRecord);
            }
        }

        public static List<ProcessInstanceRecord> ReadProcessRecord(string processName, int recordCounts)
        {
            var processInstanceRecords = new List<ProcessInstanceRecord>();
            var count = 0;

            try
            {
                lock (ThreadLocker)
                {
                    var fileInfos = ProcessRecordFileInfos();

                    if (!fileInfos.Any())
                    {
                        Log.Error("获取过程实例历史数据失败，不存在任何过程实例历史记录。");
                        return processInstanceRecords;
                    }

                    foreach (var fileInfo in fileInfos)
                    {
                        var xmlDocument = new XmlDocument();
                        xmlDocument.Load(fileInfo.FullName);

                        var root = xmlDocument.SelectSingleNode("root");
                        var selectNodes = root?.SelectNodes("ProcessInstanceRecord");

                        if (selectNodes == null) continue;
                        for (var i = selectNodes.Count - 1; i >= 0; i--)
                        {
                            var element = (XmlElement) selectNodes[i];

                            if (!element.HasAttribute("ProcessName"))
                                break;

                            var name = element.GetAttribute("ProcessName");

                            if (name != processName)
                                continue;

                            var elementOuterXml = element.OuterXml;
                            var instanceRecord = DeserializeObj<ProcessInstanceRecord>(elementOuterXml);

                            processInstanceRecords.Add(instanceRecord);

                            count++;

                            if (count >= recordCounts)
                                break;
                        }
                    }

                    return processInstanceRecords;
                }
            }
            catch (Exception e)
            {
                Log.Error($"获取过程实例历史数据失败，获取的Process为[{processName}],异常为:[{e.Message}].");

                return processInstanceRecords;
            }
        }
    }
}