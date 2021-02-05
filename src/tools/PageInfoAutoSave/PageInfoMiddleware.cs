using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Web;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.PageInfoAutoSave
{
    /// <summary>
    /// 页面信息中间件
    /// </summary>
    public class PageInfoMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => MiddlewareLevel.Last;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => ToolsOption.Instance.EnablePageInfo ? MessageHandleScope.Prepare : MessageHandleScope.None;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        async Task<bool> IMessageMiddleware.Prepare(IService service, IInlineMessage message, object tag)
        {
            if (!(tag is HttpContext context))
                return true;

            string GetHeader(string name)
            {
                if (!context.Request.Headers.TryGetValue(name, out var head))
                    return null;
                return head.ToString();
            }
            try
            {
                await MessagePoster.CallApiAsync(ToolsOption.Instance.PageInfoService, ToolsOption.Instance.PageInfoApi, new
                {
                    ApiUrl = context.Request.Path.Value,
                    Appid = GetHeader("x-zmvc-app"),
                    ActionName = GetHeader("x-zmvc-action-code"),
                    PageUrl = GetHeader("Referer"),
                    PageTitle = HttpUtility.UrlDecode(GetHeader("x-zmvc-page-title")),
                    ActionTitle = HttpUtility.UrlDecode(GetHeader("x-zmvc-action-title"))
                }.ToJson());
            }
            catch
            {
            }
            return true;
        }


    }
}