using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    public class HttpRoute
    {
        #region 初始化

        /// <summary>
        /// 选项
        /// </summary>
        public static MessageRouteOption Option { get; set; }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize(IServiceCollection services)
        {
            ConfigurationManager.RegistOnChange(() =>
            {
                Option = ConfigurationManager.Get<MessageRouteOption>("MessageMVC:HttpRoute");
            }, true);


            services.AddTransient<IMessagePoster, HttpPoster>();

            services.AddTransient<IServiceTransfer, HttpTransfer>();

            services.UseFlowByAutoDiscover();
        }

        #endregion

        #region 基本调用

        /// <summary>
        ///     POST调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task Call(HttpContext context)
        {
            /*
            在ASP.Net Core的机制中，当接收到http的头为 application/x-www-form-urlencoded 或者 multipart/form-data 时，
            netcore会通过 FormReader 预先解析 Request.Body 的 Form 的内容，经过 Reader 读取后 Request.Body 就会变 null，
            这样我们在代码中需要再次使用 Request.Body 时就会报空异常。
            */
            //context.Request.EnableRewind();

            //跨域支持
            return string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase)
                ? Task.Run(() => HttpProtocol.CrosOption(context.Response))
                : CallTask(context);
        }

        /// <summary>
        ///     调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task CallTask(HttpContext context)
        {
            try
            {
                if (context.Request.Headers.TryGetValue("User-Agent", out var agent) &&
                    agent.Count == 1 && agent[0] == MessageRouteOption.AgentName)
                {
                    await InnerCall(context);
                }
                else
                {
                    await OutCall(context);
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                try
                {
                    LogRecorder.MonitorTrace(e.Message);
                    await context.Response.WriteAsync(ApiResultHelper.BusinessErrorJson, Encoding.UTF8);
                }
                catch (Exception exception)
                {
                    LogRecorder.MonitorTrace(exception.Message);
                    LogRecorder.Exception(exception);
                }
            }
        }

        #endregion

        #region 内部调用

        /// <summary>
        /// 外部调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task InnerCall(HttpContext context)
        {
            HttpProtocol.FormatResponse(context.Request, context.Response);
            //命令
            if (context.Request.ContentLength == null || context.Request.ContentLength <= 0)
            {
                await context.Response.WriteAsync(ApiResultHelper.NotSupportJson, Encoding.UTF8);
                return;
            }
            using var texter = new StreamReader(context.Request.Body);
            var json = await texter.ReadToEndAsync();

            var message = JsonHelper.DeserializeObject<InlineMessage>(json);
            if (message == null)
            {
                await context.Response.WriteAsync(ApiResultHelper.NotSupportJson, Encoding.UTF8);
                return;
            }

            var service = ZeroFlowControl.GetService(message.ServiceName);
            if (service == null)
            {
                await context.Response.WriteAsync(ApiResultHelper.NotSupportJson, Encoding.UTF8);
                return;
            }
            await MessageProcessor.OnMessagePush(service, message, context);
        }

        #endregion

        #region 外部调用

        /// <summary>
        /// 外部调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task OutCall(HttpContext context)
        {
            HttpProtocol.CrosCall(context.Response);
            var uri = context.Request.GetUri();
            if (uri.AbsolutePath == "/")
            {
                //response.Redirect("/index.html");
                await context.Response.WriteAsync("Wecome MessageMVC,Lucky every day!", Encoding.UTF8);
                return;
            }
            HttpProtocol.FormatResponse(context.Request, context.Response);
            //命令
            var data = new HttpMessage();
            //开始调用
            if (data.CheckRequest(context))
            {
                var service = ZeroFlowControl.GetService(data.ApiHost);
                if (service == null)
                {
                    data.Result = ApiResultHelper.NoFindJson;
                }
                else if (Option.FastCall)
                {
                    await new ApiExecuter().Handle(service, data, null, null);
                }
                else
                {
                    await MessageProcessor.OnMessagePush(service, data, context);
                }
            }
        }

        #endregion


        #region 测试调用

        /// <summary>
        /// 测试调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task TestCall(HttpContext context)
        {
            HttpProtocol.CrosCall(context.Response);
            var uri = context.Request.GetUri();
            if (uri.AbsolutePath == "/")
            {
                //response.Redirect("/index.html");
                await context.Response.WriteAsync("Wecome MessageMVC,Lucky every day!", Encoding.UTF8);
                return;
            }
            HttpProtocol.FormatResponse(context.Request, context.Response);
            //命令
            var data = new HttpMessage();
            //开始调用
            if (data.CheckRequest(context))
            {
                var service = ZeroFlowControl.GetService(data.ApiHost);
                if (service == null)
                {
                    data.Result = ApiResultHelper.NoFindJson;
                    return;
                }
                data.Inline();
                var (msg, sei) = await MessagePoster.Post(data);
                await context.Response.WriteAsync(msg.Result, Encoding.UTF8);
            }
        }

        #endregion
    }
}