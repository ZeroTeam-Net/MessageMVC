﻿using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
        /// 所有节点
        /// </summary>
        public List<HttpClientItem> Clients { get; set; }


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
            ConfigurationManager.RegistOnChange<HttpClientOption>("MessageMVC:HttpClient", Instance.LoadOption, true);
        }

        void LoadOption(HttpClientOption option)
        {
            DefaultUrl = option.DefaultUrl;
            if (option.DefaultTimeOut >= 1)
                DefaultTimeOut = option.DefaultTimeOut;
            if (!Options.ContainsKey(DefaultName))
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
            if (option.Clients != null)
            {
                foreach (var item in option.Clients)
                {
                    if (string.IsNullOrEmpty(item.Services))
                    {
                        Options.Remove(item.Name);
                        continue;
                    }
                    if (Options.ContainsKey(item.Name))
                    {
                        Options[item.Name] = item;
                        continue;
                    }
                    if (item.TimeOut <= 0)
                        item.TimeOut = DefaultTimeOut;

                    Options.TryAdd(item.Name, item);

                    DependencyHelper.ServiceCollection.AddHttpClient(item.Name,client =>
                    {
                        client.BaseAddress = new Uri(item.Url);
                        client.Timeout = TimeSpan.FromSeconds(item.TimeOut);
                        client.DefaultRequestHeaders.Add("User-Agent", MessageRouteOption.AgentName);
                    });

                    foreach (var service in item.Services.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (ServiceMap.ContainsKey(service))
                            ServiceMap[service] = item.Name;
                        else
                            ServiceMap.Add(service, item.Name);
                    }
                }
            }
            DependencyHelper.Update();
            HttpClientFactory = DependencyHelper.Create<IHttpClientFactory>();
        }
    }
}
