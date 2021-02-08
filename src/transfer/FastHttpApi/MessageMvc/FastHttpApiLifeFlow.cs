using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    /// Http的流程
    /// </summary>
    public class FastHttpApiLifeFlow : IFlowMiddleware
    {
        /// <summary>
        /// 唯一实例
        /// </summary>
        public static FastHttpApiLifeFlow Instance = new FastHttpApiLifeFlow();

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(FastHttpApiLifeFlow);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Front;

        /// <summary>
        /// 当前服务
        /// </summary>
        public HttpMessageServer Server { get; private set; }

        /// <summary>
        /// 连接关闭的事件注册
        /// </summary>
        public static List<Action<ISession>> HttpDisconnectHandlers = new List<Action<ISession>>();

        private void Server_HttpDisconnect(object sender, EventArgs.SessionEventArgs e)
        {
            foreach (var handler in HttpDisconnectHandlers)
                try
                {
                    handler(e.Session);
                }
                catch (Exception ex)
                {
                    ScopeRuner.ScopeLogger.Exception(ex);
                }
        }

        Task IZeroDiscover.Discovery()
        {
            Server = new HttpMessageServer();
            if (HttpDisconnectHandlers.Count > 0)
            {
                Server.HttpDisconnect += Server_HttpDisconnect; ;
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Open()
        {
            Server.Open();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Closing()
        {
            Server.Dispose();
            return Task.CompletedTask;
        }
    }
}
