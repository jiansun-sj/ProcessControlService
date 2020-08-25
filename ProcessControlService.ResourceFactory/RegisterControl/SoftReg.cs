// ==================================================
// 文件名：SoftReg.cs
// 创建时间：2020/01/02 13:56
// ==================================================
// 最后修改于：2020/05/25 13:56
// 修改人：jians
// ==================================================

using System;
using System.Management;
using System.Text;

namespace ProcessControlService.ResourceFactory.RegisterControl
{
    internal class SoftReg
    {
        private readonly char[] _mCharAscii = new char[25]; //存储ASCII
        private readonly int[] _mIntAscii = new int[25]; //存储ASCII的值

        private readonly int[] _mIntCode = new int[127]; //存储密钥

        /// <summary>
        ///     获取硬盘序列号
        /// </summary>
        /// <returns></returns>
        private string GetDiskSerialNum()
        {
            var mydisk = new ManagementClass("win32_NetworkAdapterConfiguration");
            var disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            disk.Get();
            return disk.GetPropertyValue("VolumeSerialNumber").ToString();
        }

        /// <summary>
        ///     获取CPu序列号
        /// </summary>
        /// <returns></returns>
        private string GetCpuSerialNum()
        {
            var cpuStr = "";
            var myCpu = new ManagementClass("win32_Processor");
            var myCpuCollection = myCpu.GetInstances();
            foreach (var o in myCpuCollection)
            {
                var var = (ManagementObject) o;
                cpuStr = var.Properties["Processorid"].Value.ToString();
            }

            return cpuStr;
        }

        /// <summary>
        ///     通过CPU序列号和硬盘序列号的前24位做机器码
        /// </summary>
        /// <returns></returns>
        public string GetMachineNum()
        {
            var num = GetCpuSerialNum() + GetDiskSerialNum();
            var machineNum = num.Substring(0, 24);
            return machineNum;
        }

        /// <summary>
        ///     初始化密钥(通过模9生成)
        /// </summary>
        private void IntiIntCode()
        {
            var key = "noitamrofnIoopmuY";
            var x = Encoding.Default.GetBytes(key);
            for (var i = 0; i < _mIntCode.Length - 17; i++) _mIntCode[i] = i % 9;
            for (var i = 0; i < x.Length; i++) _mIntCode[_mIntCode.Length - 17 + i] = x[i];
        }

        /// <summary>
        ///     获取设备注册码
        /// </summary>
        /// <returns></returns>
        private string GetRegisterNum()
        {
            IntiIntCode();
            var machineNum = GetMachineNum();
            //通过机器码获取ASCII码
            for (var i = 1; i < _mCharAscii.Length; i++)
                _mCharAscii[i] = Convert.ToChar(machineNum.Substring(i - 1, 1));
            //通过简单算法，改变ASCII的值， ASCII的值，再加上初始化密钥的值
            for (var j = 1; j < _mIntAscii.Length; j++)
                _mIntAscii[j] = Convert.ToInt32(_mCharAscii[j]) + _mIntCode[Convert.ToInt32(_mCharAscii[j])];
            var machineAscii = "";
            for (var k = 1; k < _mIntAscii.Length; k++)
                if (_mIntAscii[k] >= 48 && _mIntAscii[k] <= 57 || _mIntAscii[k] >= 65 && _mIntAscii[k] <= 90 ||
                    _mIntAscii[k] >= 97 && _mIntAscii[k] <= 122)
                    machineAscii += Convert.ToChar(_mIntAscii[k]).ToString(); //在0-9,A-Z,a-z之间
                else if (_mIntAscii[k] > 122)
                    machineAscii += Convert.ToChar(_mIntAscii[k] - 10).ToString(); //大于z
                else
                    machineAscii += Convert.ToChar(_mIntAscii[k] - 9).ToString();
            return machineAscii;
        }

        public bool IsRegNumOk(string regNum)
        {
            return regNum == GetRegisterNum();
        }
    }
}