using System.Collections.Concurrent;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    /// 调用ZeroRPC的等待队列信息
    /// </summary>
    internal class ZeroRPCTaskInfo
    {
        /// <summary>
        /// TaskCompletionSource
        /// </summary>
        public TaskCompletionSource<IMessageResult> TaskSource;

        /// <summary>
        /// 调用参数
        /// </summary>
        public ZeroArgument Argument;

        /// <summary>
        /// 当前所有等待队列
        /// </summary>
        internal static readonly ConcurrentDictionary<string, ZeroRPCTaskInfo> Tasks = new ConcurrentDictionary<string, ZeroRPCTaskInfo>();

    }

}

