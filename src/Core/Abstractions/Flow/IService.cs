using System.Collections.Generic;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Services
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    public interface IService : IFlowMiddleware
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        ///     运行状态
        /// </summary>
        int RealState { get; }

        /// <summary>
        /// 消息接收对象
        /// </summary>
        public IMessageReceiver Transport { get; }

        /// <summary>
        ///     配置状态
        /// </summary>
        StationStateType ConfigState { get; set; }

        /// <summary>
        /// 是否自动发现对象
        /// </summary>
        bool IsDiscover { get; }

        /// <summary>
        /// 注册的方法
        /// </summary>
        Dictionary<string, IApiAction> Actions { get; }

    }
}