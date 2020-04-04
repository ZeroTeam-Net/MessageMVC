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
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Web
{
    /// <summary>
    /// ��Ϣת����WebSocket�м��
    /// </summary>
    public class WebSocketNotify : IMessageMiddleware
    {
        #region IMessageMiddleware

        /// <summary>
        /// ��ǰ������
        /// </summary>
        public MessageProcessor Processor { get; set; }

        /// <summary>
        /// �㼶
        /// </summary>
        int IMessageMiddleware.Level => 0;

        /// <summary>
        /// ׼��
        /// </summary>
        /// <param name="service">��ǰ����</param>
        /// <param name="message">��ǰ��Ϣ</param>
        /// <param name="tag">��չ��Ϣ</param>
        /// <param name="next">��һ��������</param>
        /// <returns></returns>
        async Task<MessageState> IMessageMiddleware.Handle(IService service, IMessageItem message, object tag, Func<Task<MessageState>> next)
        {
            if (Config.Folders.Contains(service.ServiceName))
            {
                await Publish(service.ServiceName, message.Title, message.Content);
                return MessageState.Success;
            }
            return await next();
        }

        #endregion

        #region ϵͳ����


        /// <summary>
        /// ���пͻ�������ʵ��
        /// </summary>
        internal static Dictionary<string, List<WebSocketClient>> Handlers = new Dictionary<string, List<WebSocketClient>>();


        private static WebSocketConfig _config;
        /// <summary>
        /// ���ö���
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
                    var sec = ConfigurationManager.Root.GetSection("WebSocket");
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
        /// ·�ɰ󶨴���  
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
                    TransportBuilder = name => IocHelper.Create<IMessageConsumer>()
                });
                Handlers.Add(folder, new List<WebSocketClient>());
                app.Map($"/{folder}", Map);
            }
        }

        /// <summary>
        /// �ر�
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
        /// ·�ɰ󶨴���  
        /// </summary>  
        /// <param name="app"></param>  
        private static void Map(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(Acceptor);
        }

        /// <summary>
        /// ��������ʱ������
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

        #region �㲥����

        /// <summary>
        /// �����㲥
        /// </summary>
        /// <param name="classify"></param>
        /// <param name="title"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task Publish(string classify, string title, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (!Handlers.TryGetValue(classify, out var list))
            {
                return;
            }

            var empty = string.IsNullOrWhiteSpace(title);
            var tbuffer = Encoding.UTF8.GetBytes(title);
            var title_a = new ArraySegment<byte>(tbuffer, 0, tbuffer.Length);
            var buffer = Encoding.UTF8.GetBytes(value);
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
                    if (title.IndexOf(sub, StringComparison.Ordinal) != 0)
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