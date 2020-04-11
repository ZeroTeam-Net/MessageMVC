using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.Web
{
    /// <summary>
    /// 消息转发到WebSocket中间件
    /// </summary>
    public class WebSocketNotify : IMessageMiddleware
    {
        #region IMessageMiddleware

        /// <summary>
        /// 当前处理器
        /// </summary>
        public MessageProcessor Processor { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => 0;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.End;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        async Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            if (Config.Folders.Contains(message.Topic))
            {
                await Publish(message.Offline(IocHelper.Create<IJsonSerializeProxy>()));//BUG
            }
        }

        #endregion

        #region 系统操作


        /// <summary>
        /// 所有客户端连接实例
        /// </summary>
        internal static Dictionary<string, List<WebSocketClient>> Handlers = new Dictionary<string, List<WebSocketClient>>();


        private static WebSocketConfig _config;
        /// <summary>
        /// 配置对象
        /// </summary>
        internal static WebSocketConfig Config
        {
            get
            {
                if (_config != null)
                {
                    return _config;
                }

                try
                {
                    var sec = ConfigurationManager.Root.GetSection("MessageMVC:WebSocket");
                    return _config = sec.Get<WebSocketConfig>() ?? new WebSocketConfig();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    return _config = new WebSocketConfig();
                }
            }
        }
        /// <summary>  
        /// 路由绑定处理  
        /// </summary>  
        /// <param name="app"></param>  
        public static void Binding(IApplicationBuilder app)
        {
            if (Config.Folders == null)
            {
                return;
            }

            foreach (var folder in Config.Folders)
            {
                ZeroFlowControl.RegistService(new ZeroService
                {
                    ServiceName = folder,
                    Receiver = IocHelper.Create<IMessageConsumer>()
                });
                Handlers.Add(folder, new List<WebSocketClient>());
                app.Map($"/{folder}", Map);
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static void Close()
        {
            foreach (var handler in Handlers.Values)
            {
                foreach (var client in handler)
                {
                    client.Dispose();
                }
            }
        }

        /// <summary>  
        /// 路由绑定处理  
        /// </summary>  
        /// <param name="app"></param>  
        private static void Map(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(Acceptor);
        }

        /// <summary>
        /// 接收连接时被引用
        /// </summary>
        /// <param name="hc"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static async Task Acceptor(HttpContext hc, Func<Task> n)
        {
            if (!hc.WebSockets.IsWebSocketRequest || !hc.Request.PathBase.HasValue)
            {
                return;
            }

            var classify = hc.Request.PathBase.Value.Trim('\\', '/', ' ');
            if (!Handlers.TryGetValue(classify, out var list))
            {
                return;
            }

            var socket = await hc.WebSockets.AcceptWebSocketAsync();
            var notify = new WebSocketClient(socket, classify, list);
            list.Add(notify);
            await notify.EchoLoop();
        }

        #endregion

        #region 广播管理

        /// <summary>
        /// 发出广播
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task Publish(IMessageItem message)
        {
            if (string.IsNullOrEmpty(message.Content))
            {
                return;
            }

            if (!Handlers.TryGetValue(message.Topic, out var list))
            {
                return;
            }

            var empty = string.IsNullOrWhiteSpace(message.Title);
            var tbuffer = message.Title.ToUtf8Bytes();
            var title_a = new ArraySegment<byte>(tbuffer, 0, tbuffer.Length);

            var buffer = message.Content.ToUtf8Bytes();
            var value_a = new ArraySegment<byte>(buffer, 0, buffer.Length);
            foreach (var handler in list.ToArray())
            {
                if (empty || handler.Subscriber.Count == 0)
                {
                    await handler.Send(title_a, value_a);
                    continue;
                }

                foreach (var sub in handler.Subscriber)
                {
                    if (message.Title.IndexOf(sub, StringComparison.Ordinal) != 0)
                    {
                        continue;
                    }

                    await handler.Send(title_a, value_a);
                    break;
                }
            }
        }

        #endregion

    }
}