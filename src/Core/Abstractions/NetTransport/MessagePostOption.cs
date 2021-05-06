﻿using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 消息发送器的配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MessagePostOption : IZeroOption
    {
        /// <summary>
        /// 生产者与服务关联
        /// </summary>
        [JsonProperty]
        public Dictionary<string, List<string>> PosterServices = new(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// 默认的生产者
        /// </summary>
        public IMessagePoster DefaultPoster;

        /// <summary>
        /// 服务查找表
        /// </summary>
        public readonly Dictionary<string, Dictionary<string, IMessagePoster>> ServiceMap = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 服务查找表
        /// </summary>
        public Dictionary<string, IMessagePoster> posters = new();


        #region IZeroOption

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly MessagePostOption Instance = new();

        internal static bool haseConsumer, haseProducer;

        const string sectionName = "MessageMVC:MessagePoster";


        const string optionName = "消息发送器配置";

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

        void IZeroOption.Load(bool _)
        {
            #region 配置载入

            var option = ConfigurationHelper.Get<Dictionary<string, string>>(sectionName);
            if (option == null)
            {
                return;
            }
            foreach (var kv in option)
            {
                if (kv.Value.IsMissing())
                    continue;

                if ("LocalTunnel".IsMe(kv.Key))
                    ZeroAppOption.Instance.LocalTunnel = bool.TryParse(kv.Value, out var bl) && bl;
                else if ("Default".IsMe(kv.Key))
                    ZeroAppOption.Instance.DefaultPoster = kv.Value;
                else
                    PosterServices.Add(kv.Key, kv.Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList());
            }
            #endregion
            #region 构造发送器

            posters = new Dictionary<string, IMessagePoster>();

            #endregion
        }
        internal void CheckPoster()
        {

            foreach (var poster in DependencyHelper.RootProvider.GetServices<IMessagePoster>())
            {
                posters.TryAdd(poster.PosterName, poster);
            }

            foreach (var poster in posters)
            {
                if (PosterServices.TryGetValue(poster.Key, out var services) && services != null && services.Count != 0)
                    BindingPoster(poster.Key, poster.Value, services);
            }

            if (ZeroAppOption.Instance.DefaultPoster.IsMissing() || !posters.TryGetValue(ZeroAppOption.Instance.DefaultPoster, out DefaultPoster))
            {
                var f = posters.FirstOrDefault();
                if (!f.Key.IsMissing())
                {
                    ZeroAppOption.Instance.DefaultPoster = f.Key;
                    DefaultPoster = f.Value;
                }
            }
        }
        /// <summary>
        ///     手动注销
        /// </summary>
        void BindingPoster(string posterName, IMessagePoster poster, IEnumerable<string> services)
        {
            foreach (var service in services)
            {
                if (!ServiceMap.TryGetValue(service, out var items))
                {
                    ServiceMap.Add(service, items = new Dictionary<string, IMessagePoster>());
                }
                items[posterName] = poster;
            }
        }

        /// <summary>
        ///     手动注销
        /// </summary>
        public void UnRegistPoster(string poster)
        {
            PosterServices.Remove(poster);
            posters.Remove(poster);
            foreach (var items in ServiceMap.Values)
                items.Remove(poster);
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        public void RegistPoster<TMessagePoster>(params string[] services)
            where TMessagePoster : IMessagePoster, new()
        {
            var name = typeof(TMessagePoster).GetTypeName();
            var poster = new TMessagePoster();
            posters[name] = poster;
            if (!PosterServices.ContainsKey(name))
                PosterServices.Add(name, services.ToList());
            else
                PosterServices[name].AddRange(services);
            BindingPoster(name, poster, services);
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        internal void RegistPoster(IMessagePoster poster, params string[] services)
        {
            var name = poster.GetTypeName();
            posters[name] = poster;
            if (!PosterServices.ContainsKey(name))
                PosterServices.Add(name, services.ToList());
            else
                PosterServices[name].AddRange(services);
            BindingPoster(name, poster, services);
        }

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="def">是否使用默认投递器</param>
        /// <returns>传输对象构造器</returns>
        public IMessagePoster GetService(string service, bool def)
        {
            if (ServiceMap.TryGetValue(service, out var items))
            {
                foreach (var item in items.Values)
                {
                    if (item.IsLocalReceiver && !ZeroAppOption.Instance.LocalTunnel)
                        continue;
                    return item;
                }
            }
            if (ZeroAppOption.Instance.Services.Services.TryGetValue(service, out var net))
            {
                return posters.TryGetValue(net.Poster, out var poster) ? poster : null;
            }

            return def ? DefaultPoster : null;
        }

        #endregion

    }
}
