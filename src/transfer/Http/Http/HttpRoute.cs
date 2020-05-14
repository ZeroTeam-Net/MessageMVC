using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{

    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal class HttpRoute
    {
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

        #endregion

        #region 内部调用

        /// <summary>
        /// 外部调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task InnerCall(HttpContext context)
        {
            try
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

                var message = SmartSerializer.ToMessage(json);
                if (message == null)
                {
                    await context.Response.WriteAsync(SmartSerializer.ToString(new MessageResult
                    {
                        State = MessageState.FormalError
                    }), Encoding.UTF8);
                    return;
                }

                var service = ZeroFlowControl.GetService(message.ServiceName) ??
                     new ZeroService
                     {
                         ServiceName = "***",
                         Receiver = new HttpReceiver(),
                         Serialize = DependencyHelper.GetService<ISerializeProxy>()
                     };

                await MessageProcessor.OnMessagePush(service, message, true, context);
            }
            catch (Exception e)
            {
                try
                {
                    await context.Response.WriteAsync(SmartSerializer.ToString(new MessageResult
                    {
                        State = MessageState.FrameworkError
                    }), Encoding.UTF8);
                    LogRecorder.Exception(e);
                }
                catch
                {
                }
            }
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
            try
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
                    var service = ZeroFlowControl.GetService(data.ServiceName) ??
                         new ZeroService
                         {
                             ServiceName = "***",
                             Receiver = new HttpReceiver(),
                             Serialize = DependencyHelper.GetService<ISerializeProxy>()
                         };
                    await MessageProcessor.OnMessagePush(service, data, false, context);
                }
                else
                {
                    await context.Response.WriteAsync(ApiResultHelper.NotSupportJson, Encoding.UTF8);
                    return;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                try
                {
                    await context.Response.WriteAsync(ApiResultHelper.BusinessErrorJson, Encoding.UTF8);
                }
                catch (Exception exception)
                {
                    LogRecorder.Exception(exception);
                }
            }
        }

        #endregion

    }
}