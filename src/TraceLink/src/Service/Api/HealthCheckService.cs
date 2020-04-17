#region
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.MessageTraceLink.DataAccess;
using ZeroTeam.MessageMVC.ZeroApis;
#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.WebApi
{
    /// <summary>
    ///  定时健康检查
    /// </summary>
    public partial class HealthCheckService : IFlowMiddleware
    {
        string IZeroDependency.Name => nameof(HealthCheckService);

        int IZeroMiddleware.Level => 0;

        private ILogger Logger;

        void IFlowMiddleware.Start()
        {
            Logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(HealthCheckService));
            _ = HealthCheckLoop();
        }

        private async Task HealthCheckLoop()
        {
            Logger.Information("健康检查服务启动");
            try
            {
                var time = int.Parse(ConfigurationManager.AppSettings["Time"] ?? "0");
                if (time < 1)
                {
                    time = 60000;
                }
                else
                {
                    time *= 1000;
                }

                while (ZeroFlowControl.IsAlive)
                {
                    await Task.Delay(time);
                    await HealthCheck();
                }
                Logger.Information("健康检查服务关闭");
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "健康检查服务异常退出");
            }
        }

        private async Task HealthCheck()
        {
            var now = DateTime.Now;
            int id = //int.Parse($"{now.Year}{now.Month:D2}{now.Day:D2}{now.Hour:D2}{now.Minute:D2}{now.Second:D2}");
                now.Year * 100000000 + now.Month * 1000000 + now.Day + now.Hour * 10000 + now.Minute * 100 + now.Second;
            Logger.LogInformation("{0} : 启动健康检查");
            var dirStr = ConfigurationManager.Get<HttpClientOption[]>("MessageMVC:HttpClient");
            if (dirStr == null)
            {
                return;
            }
            var serviceMap = new Dictionary<string, string>();

            foreach (var kv in dirStr)
            {
                if (string.IsNullOrEmpty(kv.Services))
                {
                    continue;
                }
                foreach (var service in kv.Services.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (serviceMap.ContainsKey(service))
                    {
                        serviceMap[service] = kv.Name;
                    }
                    else
                    {
                        serviceMap.Add(service, kv.Url);
                    }
                }
            }
            using (DependencyScope.CreateScope("HealthCheck"))
            {
                foreach (var service in serviceMap)
                {
                    await HealthCheck(id, service.Value, service.Key);
                }
            }
            Logger.LogInformation("{0} : 结束健康检查", id);
        }

        private async Task HealthCheck(int id, string url, string service)
        {
            Logger.LogInformation("健康检查 : {0}", service);
            var access = new HealthCheckDataAccess();

            var data = new HealthCheckData
            {
                CheckID = id,
                Service = service,
                Url = url,
                Start = DateTime.Now
            };
            try
            {
                var (res, seri) = await MessagePoster.Post(new InlineMessage
                {
                    ServiceName = service,
                    ApiName = "_HealthCheck_"
                });
                //res.OfflineResult(seri);
                if (res.RuntimeStatus.Code != 0)
                {
                    data.Machine = res.RuntimeStatus.Message;
                    data.Level = -1;
                    data.Details = res.RuntimeStatus.ToJson();
                }
                else
                {
                    var info = ApiResultHelper.Helper.DeserializeInterface<NameValue<Dictionary<string, HealthInfo>>>(res.Result);
                    if (!info.Success)
                    {
                        data.Level = -1;
                        data.Machine = info?.Trace.Point;
                        data.Details = res.Result;
                    }
                    else
                    {
                        data.Machine = info.ResultData.Name;
                        data.Details = res.Result;
                        if (info.ResultData.Value.Count == 0)
                        {
                            data.Level = 5;
                        }
                        else
                        {
                            data.Level = (int)info.ResultData.Value.Values.Average(p => p.Level);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                data.Machine = "Exception";
                data.Level = -1;
                data.Details = ex.Message;
                Logger.Exception(ex, "结果检查 : {0}", service);
            }
            data.End = DateTime.Now;
            try
            {
                await access.InsertAsync(data);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "写入结果 : {0}", service);
            }
        }
    }

    /// <summary>
    /// HttpClient预定义服务映射配置
    /// </summary>
    internal class HttpClientOption
    {
        /// <summary>
        /// 别名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 基础地址,包含http://
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 绑定的服务列表,组合结果为 [Url]/[Service]/[ApiName]
        /// </summary>
        public string Services { get; set; }
    }
}