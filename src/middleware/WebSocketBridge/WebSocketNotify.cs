using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => MiddlewareLevel.Basic;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Prepare;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        async Task<bool> IMessageMiddleware.Prepare(IService service, IInlineMessage message, object tag)
        {
            if (!Config.Folders.Contains(message.Topic))
            {
                return true;
            }
            message.Offline();
            await Publish(message);
            message.State = MessageState.NoUs;
            return false;
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
                    var sec = ConfigurationHelper.Root.GetSection("MessageMVC:WebSocket");
                    return _config = sec.Get<WebSocketConfig>() ?? new WebSocketConfig();
                }
                catch (Exception e)
                {
                    DependencyScope.Logger.Exception(e);
                    return _config = new WebSocketConfig();
                }
            }
        }

        /// <summary>  
        /// 路由绑定处理  
        /// </summary>
        public static void CreateService()
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
                    Receiver = DependencyHelper.GetService<INetEvent>()
                });
                Handlers.Add(folder, new List<WebSocketClient>());
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
        internal static void Map(IApplicationBuilder app)
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
            await SendLast(notify);
            await notify.EchoLoop();
        }

        #endregion

        #region 广播管理

        /// <summary>
        /// 最后一条消息
        /// </summary>
        static readonly Dictionary<string, IMessageItem> Last = new Dictionary<string, IMessageItem>();

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
            Last[message.Topic] = message;
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
                await SendTo(message, handler, empty, title_a, value_a);
        }

        private static async Task SendLast(WebSocketClient handler)
        {
            foreach (var message in Last.Values)
            {
                await SendTo(message, handler);
            }
        }

        private static async Task SendTo(IMessageItem message, WebSocketClient handler)
        {
            var empty = string.IsNullOrWhiteSpace(message.Title);
            var tbuffer = message.Title.ToUtf8Bytes();
            var title_a = new ArraySegment<byte>(tbuffer, 0, tbuffer.Length);

            var buffer = message.Content.ToUtf8Bytes();
            var value_a = new ArraySegment<byte>(buffer, 0, buffer.Length);
            await SendTo(message, handler, empty, title_a, value_a);
        }

        private static async Task SendTo(IMessageItem message, WebSocketClient handler, bool empty, ArraySegment<byte> title_a, ArraySegment<byte> value_a)
        {
            if (empty || handler.Subscriber.Count == 0)
            {
                await handler.Send(title_a, value_a);
                return;
            }

            foreach (var sub in handler.Subscriber)
            {
                if (message.Title.IndexOf(sub, StringComparison.Ordinal) == 0)
                {
                    await handler.Send(title_a, value_a);
                    break;
                }
            }
        }

        #endregion

    }
}