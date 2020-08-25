// ==================================================
// 文件名：CheckReg.cs
// 创建时间：2020/01/02 13:53
// ==================================================
// 最后修改于：2020/05/25 13:53
// 修改人：jians
// ==================================================

using System;
using Microsoft.Win32;

namespace ProcessControlService.ResourceFactory.RegisterControl
{
    public class CheckReg
    {
        private readonly SoftReg _softReg = new SoftReg();

        /// <summary>
        ///     检查是否已经注册
        /// </summary>
        /// <returns></returns>
        public bool GetIsReg()
        {
            var isCheck = false;
            var regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true)?.CreateSubKey("Hosting")
                ?.CreateSubKey("Register.INI");
            if (regKey != null)
                foreach (var item in regKey.GetSubKeyNames())
                    if (_softReg.IsRegNumOk(item))
                        isCheck = true;
            return isCheck;
        }


        /// <summary>
        ///     判断软件是否可用，拥有二十次的试用期，也可以换成天数,再写入注册表信息
        /// </summary>
        /// <returns></returns>
        public bool GetUseInfo(ref int mIntUse)
        {
            if (mIntUse <= 0) throw new ArgumentOutOfRangeException(nameof(mIntUse));
            //待修改
            mIntUse = 0;
            var isCanUse = false;
            try
            {
                mIntUse = (int) Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Hosting", "UseTimes", 0);
            }
            catch (Exception)
            {
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Hosting", "UseTimes", 0, RegistryValueKind.DWord);
            }

            mIntUse = (int) Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Hosting", "UseTimes", 0);
            if (mIntUse < 5)
            {
                var intCount = mIntUse + 1;
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Hosting", "UseTimes", intCount);
                isCanUse = true;
            }
            else
            {
                isCanUse = false;
            }

            return isCanUse;
        }

        public string GetMachineNum()
        {
            return _softReg.GetMachineNum();
        }

        public bool IsRegnumOK(string reg)
        {
            if (_softReg.IsRegNumOk(reg))
                return true;
            return false;
        }
    }
}