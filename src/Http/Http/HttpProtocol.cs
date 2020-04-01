using Microsoft.AspNetCore.Http;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// HTTPЭ����ص�֧��
    /// </summary>
    public class HttpProtocol
    {
        /// <summary>
        ///     ����֧��
        /// </summary>
        internal static void CrosOption(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET", "POST" });
            response.Headers.Add("Access-Control-Allow-Headers", new[] { "x-requested-with", "content-type", "authorization", "*" });
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        /// <summary>
        ///     ����֧��
        /// </summary>
        internal static void CrosCall(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        /// <summary>
        ///     ��������
        /// </summary>
        internal static void FormatResponse(HttpRequest request, HttpResponse response)
        {
            response.Headers["Content-Type"] = response.ContentType = "application/json; charset=UTF-8";
        }
    }
}