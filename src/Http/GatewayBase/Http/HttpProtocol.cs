using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace MicroZero.Http.Gateway
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