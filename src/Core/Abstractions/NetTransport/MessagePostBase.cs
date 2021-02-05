using Agebull.Common.Ioc;
using Microsoft.Extensions.Logging;
using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息投递器基类
    /// </summary>
    public class MessagePostBase
    {
        /// <summary>
        /// 构造
        /// </summary>
        protected MessagePostBase()
        {
            Logger = DependencyHelper.LoggerFactory.CreateLogger(GetType().GetTypeName());
            State = StationStateType.Run;
        }

        /// <summary>
        /// 是否本地接收者
        /// </summary>
        public bool IsLocalReceiver => false;

        /// <summary>
        /// 日志器
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// 运行状态
        /// </summary>
        public StationStateType State { get; set; }

    }
}
