using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// HttpClient预定义服务映射配置
    /// </summary>
    internal class HttpClientOption : IZeroOption
    {
        /// <summary>
        /// 默认地址
        /// </summary>
        public string DefaultUrl { get; set; }

        /// <summary>
        /// 默认超时（秒）
        /// </summary>
        public int DefaultTimeOut { get; set; }

        /// <summary>
        /// 可以包含的跟踪信息
        /// </summary>
        public MessageTraceType IncludeTrace { get; set; }

        /// <summary>
        /// 所有服务
        /// </summary>
        public List<HttpClientItem> Services { get; set; }


        internal static Dictionary<string, HttpClientItem> Options = new Dictionary<string, HttpClientItem>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 服务到HttpClientName的查找表.
        /// </summary>
        internal static readonly Dictionary<string, string> ServiceMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static IHttpClientFactory httpClientFactory;

        internal static IHttpClientFactory HttpClientFactory => httpClientFactory ??= DependencyHelper.GetService<IHttpClientFactory>();

        public const string DefaultName = "_default_";

        #region IZeroOption

        /// <summary>
        /// 实例
        /// </summary>
        public static HttpClientOption Instance = new HttpClientOption();


        const string sectionName = "HttpClient";

        const string optionName = "HttpClient配置";

        const string supperUrl = "https://";

        /// <summary>
        /// 支持地址
        /// </summary>
        string IZeroOption.SupperUrl => supperUrl;

        /// <summary>
        /// 配置名称
        /// </summary>
        string IZeroOption.OptionName => optionName;


        /// <summary>
        /// 节点名称
        /// </summary>
        string IZeroOption.SectionName => sectionName;

        /// <summary>
        /// 是否动态配置
        /// </summary>
        bool IZeroOption.IsDynamic => false;

        void IZeroOption.Load(bool first)
        {
            var option = ConfigurationHelper.Get<HttpClientOption>(sectionName);
            if (option == null)
                option = ConfigurationHelper.Get<HttpClientOption>("Http:Client");
            if (option == null)
                return;

            if (option.DefaultUrl.IsMissing())
                throw new ZeroOptionException(optionName, sectionName, "DefaultUrl不能为空");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            DefaultUrl = option.DefaultUrl;
            if (option.DefaultTimeOut >= 1)
                DefaultTimeOut = option.DefaultTimeOut;
            else
                DefaultTimeOut = 30;
            Options.TryAdd(DefaultName, new HttpClientItem
            {
                Name = DefaultName,
                Url = DefaultUrl,
                TimeOut = DefaultTimeOut
            });
            DependencyHelper.ServiceCollection.AddHttpClient(DefaultName, client =>
            {
                client.BaseAddress = new Uri(DefaultUrl);
                client.Timeout = TimeSpan.FromSeconds(DefaultTimeOut);
                client.DefaultRequestHeaders.Add("User-Agent", MessageRouteOption.AgentName);
            });
            if (option.Services == null)
            {
                return;
            }
            foreach (var item in option.Services)
            {
                foreach (var service in ServiceMap.Where(p => p.Value == item.Name).Select(p => p.Key).ToArray())
                    ServiceMap.Remove(service);

                if (item.TimeOut <= 0)
                    item.TimeOut = DefaultTimeOut;

                if (Options.ContainsKey(item.Name))
                {
                    Options[item.Name] = item;
                }
                else
                {
                    Options.TryAdd(item.Name, item);

                    DependencyHelper.ServiceCollection.AddHttpClient(item.Name, client =>
                    {
                        client.BaseAddress = new Uri(item.Url);
                        client.Timeout = TimeSpan.FromSeconds(item.TimeOut);
                            //client.DefaultRequestHeaders.Add("Content-Type", item.ContentType ?? "application/json;charset=utf-8");
                            client.DefaultRequestHeaders.Add("User-Agent", item.UserAgent ?? MessageRouteOption.AgentName);
                    });
                }

                ServiceMap.Add(item.Name, item.Name);
                if (string.IsNullOrEmpty(item.Alias))
                {
                    continue;
                }
                foreach (var service in item.Alias.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (ServiceMap.ContainsKey(service))
                        ServiceMap[service] = item.Name;
                    else
                        ServiceMap.Add(service, item.Name);
                }
            }
            DependencyHelper.Logger.Information("HttpPost已开启");
        }
        #endregion
    }
}
