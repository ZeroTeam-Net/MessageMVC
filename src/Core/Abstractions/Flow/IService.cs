using ZeroTeam.MessageMVC.Documents;
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
        string ServiceName { get; set; }

        /// <summary>
        ///     运行状态
        /// </summary>
        int RealState { get; }

        /// <summary>
        /// 序列化对象
        /// </summary>
        ISerializeProxy Serialize { get; set; }

        /// <summary>
        /// 消息接收对象
        /// </summary>
        IMessageReceiver Receiver { get; set; }

        /// <summary>
        ///     配置状态
        /// </summary>
        StationStateType ConfigState { get; set; }

        /// <summary>
        /// 是否自动发现对象
        /// </summary>
        bool IsAutoService { get; set; }

        /// <summary>
        ///  取得API信息
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        IApiAction GetApiAction(string api);

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="info">反射信息</param>
        void RegistAction(string name, ApiActionInfo info);

        /// <summary>
        /// 重置状态机,请谨慎使用
        /// </summary>
        void ResetStateMachine();
    }
}