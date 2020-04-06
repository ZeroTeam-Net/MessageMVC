using Microsoft.AspNetCore.Http;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// HTTP协议相关的支持
    /// </summary>
    public class HttpProtocol
    {
        /// <summary>
        ///     跨域支持
        /// </summary>
        internal static void CrosOption(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET", "POST" });
            response.Headers.Add("Access-Control-Allow-Headers", new[] { "x-requested-with", "content-type", "authorization", "*" });
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        /// <summary>
        ///     跨域支持
        /// </summary>
        internal static void CrosCall(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        /// <summary>
        ///     返回类型
        /// </summary>
        internal static void FormatResponse(HttpRequest request, HttpResponse response)
        {
            response.Headers["Content-Type"] = response.ContentType = "application/json; charset=UTF-8";
        }
    }
}