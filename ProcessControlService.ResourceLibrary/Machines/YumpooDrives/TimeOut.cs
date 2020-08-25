using System;
using System.Net.Sockets;
using YumpooDrive.Core;

namespace YumpooDrive
{
    /// <summary>
    /// 超时操作的类 [a class use to indicate the time-out of the connection]
    /// </summary>
    internal class TimeOut
    {
        /// <summary>
        /// 操作的开始时间
        /// </summary>
        public DateTime StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSuccessful
        {
            get;
            set;
        }

        /// <summary>
        /// 延时的时间，单位毫秒
        /// </summary>
        public int DelayTime
        {
            get;
            set;
        }

        /// <summary>
        /// 连接超时用的Socket
        /// </summary>
        public Socket WorkSocket
        {
            get;
            set;
        }

        /// <summary>
        /// 用于超时执行的方法
        /// </summary>
        public Action Operator
        {
            get;
            set;
        }

        /// <summary>
        /// 当前对象判断的同步锁
        /// </summary>
        public SimpleHybirdLock HybirdLock
        {
            get;
            set;
        }

        /// <summary>
        /// 实例化对象
        /// </summary>
        public TimeOut()
        {
            StartTime = DateTime.Now;
            IsSuccessful = false;
            HybirdLock = new SimpleHybirdLock();
        }
    }
}
