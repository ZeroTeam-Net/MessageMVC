using Microsoft.AspNetCore.Http;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// HTTP协议相关的支持
    /// </summary>
    internal class HttpProtocol
    {
        static readonly string[] methods = new[] { "GET", "POST", "OPTIONS" };
        static readonly string[] headers = new[]
        {
            "x-requested-with",
            "content-type",
            "authorization",
            "x-zmvc-app",
            "x-zmvc-page-title",
            "x-zmvc-action-code",
            "x-zmvc-action-title",
            "*"
        };

        /// <summary>
        ///     跨域支持
        /// </summary>
        internal static void CrosOption(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Methods", "*");
            response.Headers.Add("Access-Control-Allow-Headers", "*");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        /// <summary>
        ///     跨域支持
        /// </summary>
        internal static void CrosCall(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }
    }
}