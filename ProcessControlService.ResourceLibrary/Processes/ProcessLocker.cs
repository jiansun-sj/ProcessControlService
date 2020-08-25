// ==================================================
// 文件名：ProcessLocker.cs
// 创建时间：2020/06/11 11:48
// ==================================================
// 最后修改于：2020/06/11 11:48
// 修改人：jians
// ==================================================

using System;
using System.Xml;

namespace ProcessControlService.ResourceLibrary.Processes
{
    public class ProcessLocker
    {
        public short EntryLockerStep { get; set; }

        public short ExitLockerStep { get; set; }

        public string LockerKey { get; set; }

        public static ProcessLocker LoadFromConfig(XmlElement xmlElement)
        {
            var processLocker = new ProcessLocker
            {
                EntryLockerStep = Convert.ToInt16(xmlElement.GetAttribute(nameof(EntryLockerStep))),
                ExitLockerStep = Convert.ToInt16(xmlElement.GetAttribute(nameof(ExitLockerStep))),
                LockerKey = xmlElement.GetAttribute(nameof(LockerKey))
            };

            if (processLocker.EntryLockerStep<=0|| processLocker.ExitLockerStep<=0 || string.IsNullOrEmpty(processLocker.LockerKey))
            {
                throw new ArgumentException("ProcessLocker申明参数不合法，请检查Locker参数设置。");
            }

            if (processLocker.ExitLockerStep<processLocker.EntryLockerStep)
            {
                throw new ArgumentException($"ProcessLocker申明参数不合法,解锁步骤Id不能小于进锁步骤Id");
            }
            
            return processLocker;
        }
    }
}