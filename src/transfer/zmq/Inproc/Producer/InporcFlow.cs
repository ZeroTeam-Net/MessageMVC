using Agebull.Common;
using System.Collections.Concurrent;
using ZeroTeam.ZeroMQ;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace ZeroTeam.MessageMVC.ZeroMQ.Inporc
{
    /// <summary>
    ///     ZMQ环境流程处理
    /// </summary>
    public class InporcFlow : IFlowMiddleware
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
            return ZSocketEx.CreateOnceSocket(InprocAddress, null, name.ToBytes(), ZSocketType.PAIR);
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
        public static InporcFlow Instance = new InporcFlow();

        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize()
        {
            ZContext.Initialize();
            InprocPoster.Instance.State = StationStateType.Run;
        }

        /// <summary>
        /// 注销时调用
        /// </summary>
        public void Close()
        {
            InprocPoster.Instance.State = StationStateType.Closed;
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

        internal static readonly ConcurrentDictionary<ulong, TaskInfo> Tasks = new ConcurrentDictionary<ulong, TaskInfo>();

        #endregion
    }
}

