using Microsoft.AspNetCore.Builder;

namespace ZeroTeam.MessageMVC.Web
{
    /// <summary>
    /// WebSocket消息转发应用
    /// </summary>
    public static class WebSocketNotifyApplication
    {

        /// <summary>  
        /// 路由绑定处理  
        /// </summary>  
        /// <param name="app"></param>  
        public static void UseWebSocketNotify(this IApplicationBuilder app)
        {
            if (WebSocketNotify.Config.Folders == null)
            {
                return;
            }
            foreach (var folder in WebSocketNotify.Config.Folders)
            {
                app.Map($"/{folder}", WebSocketNotify.Map);
            }
            WebSocketNotify.CreateService();
        }
    }
}
