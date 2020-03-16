using System;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    public interface IService : IDisposable
    {
        /// <summary>
        /// 实例名称
        /// </summary>
        string InstanceName { get; }

        /// <summary>
        /// 服务名称
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        ///     运行状态
        /// </summary>
        int RealState { get; }

        /// <summary>
        ///     配置状态
        /// </summary>
        StationStateType ConfigState { get; }

        /// <summary>
        /// 系统初始化时调用
        /// </summary>
        void OnInitialize();

        /// <summary>
        /// 系统启动时调用
        /// </summary>
        bool OnStart();

        /// <summary>
        /// 系统关闭时调用
        /// </summary>
        bool OnEnd();

        /// <summary>
        /// 注销时调用
        /// </summary>
        void OnDestory();

        /// <summary>
        /// 开启
        /// </summary>
        bool Start();

        /// <summary>
        /// 关闭
        /// </summary>
        bool Close();

    }

}