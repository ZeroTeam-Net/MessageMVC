using System;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// 注册的方法
        /// </summary>
        Dictionary<string, IApiAction> Actions { get; }

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
        /// 网络传输对象
        /// </summary>
        public INetTransport Transport { get; }

        /// <summary>
        ///     配置状态
        /// </summary>
        StationStateType ConfigState { get; }

        /// <summary>
        /// 系统初始化时调用
        /// </summary>
        void Initialize();


        /// <summary>
        /// 开启
        /// </summary>
        bool Start();

        /// <summary>
        /// 关闭
        /// </summary>
        bool Close();

        /// <summary>
        /// 结束
        /// </summary>
        void End();
        

    }

}