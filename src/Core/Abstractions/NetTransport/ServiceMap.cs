using Agebull.Common.Ioc;
using System;
using System.Collections.Generic;
using System.Reflection;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 服务字典
    /// </summary>
    public class ServiceMap
    {
        /// <summary>
        /// 合并
        /// </summary>
        public void Merge(Dictionary<string, string[]> map)
        {
            if (map == null)
            {
                return;
            }
            foreach (var kv in map)
            {
                var item = new ServiceItem();
                if(!NetTransfers.TryAdd(kv.Key, item))
                {
                    Console.WriteLine(kv.Key);
                }
                item = NetTransfers[kv.Key];
                if (kv.Value == null || kv.Value.Length == 0)
                    continue;
                foreach (var service in kv.Value)
                {
                    Services[service] = item;
                }
            }
        }

        /// <summary>
        /// 关联的服务
        /// </summary>
        public readonly Dictionary<string, ServiceItem> NetTransfers = new();

        /// <summary>
        /// 关联的服务
        /// </summary>
        public readonly Dictionary<string, ServiceItem> Services = new();

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="transfer"></param>
        /// <param name="poster"></param>
        /// <param name="Receiver"></param>
        public void Regist(string transfer, string poster, Func<IMessageReceiver> Receiver)
        {
            if(!NetTransfers.TryGetValue(transfer,out var item))
            {
                NetTransfers.Add(transfer,new ServiceItem
                {
                    Poster = poster,
                    Receiver = Receiver
                });
            }
            else
            {
                item.Poster = poster;
                item.Receiver = Receiver;
            }
        }

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="type">控制器类型</param>
        /// <param name="name">发现的服务名称</param>
        /// <returns>传输对象构造器</returns>
        public Func<string, IMessageReceiver> DiscoverNetTransport(Type type, out string name)
        {
            foreach (var attr in type.GetCustomAttributes())
            {
                if (attr is IReceiverGet receiverGet)
                {
                    name = receiverGet.ServiceName;
                    if (Services.TryGetValue(name, out var item))
                    {
                        return name => item.Receiver();
                    }
                    return receiverGet.Receiver;
                }
            }
            name = ZeroAppOption.Instance.ApiServiceName;
            if (Services.TryGetValue(name, out var item2))
            {
                return name => item2.Receiver();
            }
            return name => DependencyHelper.GetService<IServiceReceiver>();
        }
    }

    /// <summary>
    /// 服务字典
    /// </summary>
    public class ServiceItem
    {
        /// <summary>
        /// 发送器构造
        /// </summary>
        public string Poster { get; set; }

        /// <summary>
        /// 接收器构造
        /// </summary>
        public Func<IMessageReceiver> Receiver { get; set; }
    }
}