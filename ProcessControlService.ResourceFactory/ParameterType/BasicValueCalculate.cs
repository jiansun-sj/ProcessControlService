// ==================================================
// 文件名：BasicValueCalculate.cs
// 创建时间：2020/01/02 10:53
// ==================================================
// 最后修改于：2020/06/08 10:53
// 修改人：jians
// ==================================================

using System;
using log4net;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    ///     基本类型计算
    /// </summary>
    public class BasicValueCalculateD2
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BasicValueCalculateD2));
        
        public static dynamic Calculate(BasicValueCalculateType calcType, dynamic value1, dynamic value2)
        {
            switch (calcType)
            {
                case BasicValueCalculateType.add:
                    return Add(value1, value2);

                default:
                    return default;
            }
        }

        private static dynamic Add(dynamic value1, dynamic value2)
        {
            if (value1.GetType() != value2.GetType())
                throw new InvalidOperationException($"{value1},{value2}不是同类型参数,不允许执行基本类型Add操作");

            if (value1 is short a1 && value2 is short b1) return (short)(a1 + b1);

            if (value1 is int a2 && value2 is int b2) return a2 + b2;

            return default;
        }

        public static BasicValueCalculateType GetCalculateType(string strCalculateType)
        {
            try
            {
                return (BasicValueCalculateType) Enum.Parse(typeof(BasicValueCalculateType),
                    strCalculateType.ToLower());
            }
            catch (Exception ex)
            {
                Log.Error($"基本数值类数值计算类型有错，异常为：[{ex}]。");
                return BasicValueCalculateType.unknown;
            }
        }
    }

    public enum BasicValueCalculateType
    {
        add = 0,
        minus = 1,
        multiplication = 2,
        division = 3,
        unknown = 10
    }
}