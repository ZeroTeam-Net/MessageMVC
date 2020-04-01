using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    public interface IService : IFlowMiddleware
    {
        /// <summary>
        /// 注册的方法
        /// </summary>
        Dictionary<string, IApiAction> Actions { get; }

        /// <summary>
        /// 服务名称
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        ///     运行状态
        /// </summary>
        int RealState { get; }

        /// <summary>
        /// 网络传输对象
        /// </summary>
        public INetTransfer Transport { get; }

        /// <summary>
        ///     配置状态
        /// </summary>
        StationStateType ConfigState { get; set; }

        /// <summary>
        /// 重置状态机,请谨慎使用
        /// </summary>
        void ResetStateMachine();
    }
}