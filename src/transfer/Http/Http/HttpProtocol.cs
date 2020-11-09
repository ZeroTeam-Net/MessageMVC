using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// HTTP协议相关的支持
    /// </summary>
    internal class HttpProtocol
    {
        static string[] methods = new[] { "GET", "POST" };
        static string[] headers = new[] { "x-requested-with", "content-type", "authorization", "*" };
        /// <summary>
        ///     跨域支持
        /// </summary>
        internal static void CrosOption(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Methods", methods);
            response.Headers.Add("Access-Control-Allow-Headers", headers);
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