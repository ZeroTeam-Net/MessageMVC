using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroMQ.Inporc
{
    internal class TaskInfo
    {
        /// <summary>
        /// TaskCompletionSource
        /// </summary>
        public TaskCompletionSource<IInlineMessage> TaskSource;

        /// <summary>
        /// 任务开始时间
        /// </summary>
        public DateTime Start;

        /// <summary>
        /// 调用对象
        /// </summary>
        public ZmqCaller Caller;
    }
}

