using System;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
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
    ///     调用映射核心类
    /// </summary>
    public class MessageRoute
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
            Option = ConfigurationManager.Get<MessageRouteOption>("Route");

            services.AddTransient<IRpcTransfer, HttpTransfer>();

            services.UseFlowByAutoDiscory();

        }

        /*// <summary>
        ///     刷新
        /// </summary>
        public static void Flush()
        {
            RouteCache.Flush();
        }

        /// <summary>
        /// 配置HTTP
        /// </summary>
        /// <param name="options"></param>
        public static void Options(KestrelServerOptions options)
        {
            options.AddServerHeader = true;
            //将此选项设置为 null 表示不应强制执行最低数据速率。
            options.Limits.MinResponseDataRate = null;

            var httpOptions = ConfigurationManager.Root.GetSection("http").Get<HttpOption[]>();
            foreach (var option in httpOptions)
            {
                if (option.IsHttps)
                {
                    var filename = option.CerFile[0] == '/'
                        ? option.CerFile
                        : Path.Combine(Environment.CurrentDirectory, option.CerFile);
                    var certificate = new X509Certificate2(filename, option.CerPwd);
                    options.Listen(IPAddress.Any, option.Port, listenOptions =>
                    {
                        listenOptions.UseHttps(certificate);
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                }
                else
                {
                    options.Listen(IPAddress.Any, option.Port, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                }
            }
        }*/
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
            using (MonitorScope.CreateScope(uri.AbsolutePath))
            {
                var data = new RouteData();
                try
                {
                    //开始调用
                    if (await data.CheckRequest(context))
                    {
                        var service = ZeroFlowControl.GetService(data.ApiHost);
                        if (service == null)
                            data.Result = ApiResultIoc.NoFindJson;
                        else if(Option.FastCall)
                            await new ApiExecuter().Handle(service, data, null, null);
                        else
                            await MessageProcess.OnMessagePush(service, data, data);
                    }
                    // 写入返回
                    await context.Response.WriteAsync(
                        data.Result ?? (data.Result = ApiResultIoc.RemoteEmptyErrorJson),
                        Encoding.UTF8);
                }
                catch (Exception e)
                {
                    await OnError(e, context);
                }
            }
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="e"></param>
        /// <param name="context"></param>
        static async Task OnError(Exception e, HttpContext context)
        {
            try
            {
                LogRecorder.MonitorTrace(e.Message);
                //Data.UserState = UserOperatorStateType.LocalException;
                //Data.ZeroState = ZeroOperatorStateType.LocalException;
                ZeroTrace.WriteException("Route", e);
                ////IocHelper.Create<IRuntimeWaring>()?.Waring("Route", Data.Uri.LocalPath, e.Message);
                await context.Response.WriteAsync(ApiResultIoc.LocalErrorJson, Encoding.UTF8);
            }
            catch (Exception exception)
            {
                LogRecorder.Exception(exception);
            }
        }
        #endregion

    }
}