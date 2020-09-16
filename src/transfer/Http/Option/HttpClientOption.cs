using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
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
    internal class HttpClientOption
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
        /// 所有服务
        /// </summary>
        public List<HttpClientItem> Services { get; set; }


        /// <summary>
        /// 所有节点
        /// </summary>
        public static HttpClientOption Instance = new HttpClientOption
        {
            DefaultTimeOut = 30
        };


        internal static Dictionary<string, HttpClientItem> Options = new Dictionary<string, HttpClientItem>();

        /// <summary>
        /// 服务到HttpClientName的查找表.
        /// </summary>
        internal static readonly Dictionary<string, string> ServiceMap = new Dictionary<string, string>();

        internal static IHttpClientFactory HttpClientFactory;

        public const string DefaultName = "_default_";

        static HttpClientOption()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            ConfigurationHelper.RegistOnChange<HttpClientOption>("Http:Client", Instance.LoadOption, true);
        }
        bool isLoaded;
        void LoadOption(HttpClientOption option)
        {
            DefaultUrl = option.DefaultUrl;
            if (option.DefaultTimeOut >= 1)
                DefaultTimeOut = option.DefaultTimeOut;

            if (!isLoaded && !Options.ContainsKey(DefaultName))
            {
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
            }
            if (option.Services != null)
            {
                foreach (var item in option.Services)
                {
                    foreach (var service in ServiceMap.Where(p => p.Value == item.Name).Select(p => p.Key).ToArray())
                        ServiceMap.Remove(service);

                    if (string.IsNullOrEmpty(item.Alias))
                    {
                        Options.Remove(item.Name);
                        continue;
                    }
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
                            client.DefaultRequestHeaders.Add("Content-Type", item.ContentType ?? "application/json;charset=utf-8");
                            client.DefaultRequestHeaders.Add("User-Agent", item.UserAgent ?? MessageRouteOption.AgentName);
                        });
                    }

                    foreach (var service in item.Alias.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (ServiceMap.ContainsKey(service))
                            ServiceMap[service] = item.Name;
                        else
                            ServiceMap.Add(service, item.Name);
                    }
                }
            }
            if (!isLoaded)
                DependencyHelper.Reload();
            isLoaded = true;
            HttpClientFactory = DependencyHelper.GetService<IHttpClientFactory>();
        }
    }
}
