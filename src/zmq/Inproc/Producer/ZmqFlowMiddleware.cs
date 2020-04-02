using Agebull.Common;
using Agebull.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ZeroTeam.ZeroMQ;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace ZeroTeam.MessageMVC.ZeroMQ.Inporc
{
    /// <summary>
    ///     ZMQ环境流程处理
    /// </summary>
    public class ZmqFlowMiddleware : IFlowMiddleware
    {
        #region Socket

        /// <summary>
        /// 代理地址
        /// </summary>
        internal const string InprocAddress = "inproc://zmq.req";

        /// <summary>
        /// 服务令牌
        /// </summary>
        internal static byte[] ServiceKey = new[] { (byte)0 };

        /// <summary>
        /// 取得连接器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ZSocketEx GetProxySocket(string name = null)
        {
            return ZSocketEx.CreateOnceSocket(InprocAddress, null, name.ToZeroBytes(), ZSocketType.PAIR);
        }

        #endregion

        #region 流程

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroMiddleware.Name => "ZMQProxy";

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => short.MinValue;

        /*// <summary>
        /// 等待数量
        /// </summary>
        public int WaitCount;*/

        /// <summary>
        /// 实例
        /// </summary>
        public static ZmqFlowMiddleware Instance { get; private set; }

        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize()
        {
            Instance = this;
            ZContext.Initialize();
            InporcProducer.Instance.State = StationStateType.Run;
        }

        /// <summary>
        /// 注销时调用
        /// </summary>
        public void Close()
        {
            InporcProducer.Instance.State = StationStateType.Closed;
        }

        /// <summary>
        /// 注销时调用
        /// </summary>
        public void End()
        {
            ZContext.Destroy();
        }

        #endregion

        #region 异步

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal Task<ZeroResult> CallZero(ZmqCaller caller, byte[] description, params byte[][] args)
        {
            var info = new TaskInfo
            {
                Caller = caller,
                Start = DateTime.Now
            };
            if (!Tasks.TryAdd(caller.Name, info))
            {
                return Task.FromResult(new ZeroResult
                {
                    State = ZeroOperatorStateType.NoWorker,
                });
            }
            using (MonitorScope.CreateScope("SendToZero"))
            {
                using var message = new ZMessage
                {
                    new ZFrame(description)
                };
                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        message.Add(new ZFrame(arg));
                    }
                    //message.Add(new ZFrame(caller.Config.ServiceKey));
                }

                bool res;
                ZError error;
                using (var socket = ZSocketEx.CreateOnceSocket(InprocAddress, null, caller.Name.ToString().ToZeroBytes(), ZSocketType.PAIR))
                {
                    res = socket.Send(message, out error);
                }
                if (!res)
                {
                    return Task.FromResult(new ZeroResult
                    {
                        State = ZeroOperatorStateType.LocalSendError,
                        ZmqError = error
                    });
                }
            }

            info.TaskSource = new TaskCompletionSource<ZeroResult>();
            return info.TaskSource.Task;
        }

        internal readonly ConcurrentDictionary<long, TaskInfo> Tasks = new ConcurrentDictionary<long, TaskInfo>();


        internal class TaskInfo
        {
            /// <summary>
            /// TaskCompletionSource
            /// </summary>
            public TaskCompletionSource<ZeroResult> TaskSource;

            /// <summary>
            /// 任务开始时间
            /// </summary>
            public DateTime Start;

            /// <summary>
            /// 调用对象
            /// </summary>
            public ZmqCaller Caller;
        }

        #endregion
    }

}

