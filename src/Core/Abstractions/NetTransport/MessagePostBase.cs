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
        /// 日志器
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// 运行状态
        /// </summary>
        public StationStateType State { get; set; }

        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize()
        {
            Logger = DependencyHelper.LoggerFactory.CreateLogger(GetType().GetTypeName());
            State = StationStateType.Run;
        }
    }
}
