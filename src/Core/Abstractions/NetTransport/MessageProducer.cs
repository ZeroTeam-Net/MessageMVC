using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息生产对象
    /// </summary>
    public class MessageProducer : IFlowMiddleware
    {
        /// <summary>
        /// 默认的生产者
        /// </summary>
        public static IMessageProducer Default { get; set; }

        string IFlowMiddleware.RealName => "ServiceSeletctor";

        int IFlowMiddleware.Level => int.MaxValue;

        static Dictionary<string, IMessageProducer> Dictionary = new Dictionary<string, IMessageProducer>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     初始化
        /// </summary>
        void IFlowMiddleware.Initialize()
        {
            var pros = IocHelper.RootProvider.GetServices<IMessageProducer>().ToArray();
            if (pros.Length == 1)
            {
                Default = pros[0];
                return;
            }
            var sec = ConfigurationManager.Get("MessageProducer");
            foreach (var pro in pros)
            {
                var strs = sec.GetStr(pro.GetTypeName(), "")?.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in strs)
                {
                    Dictionary.TryAdd(str, pro);
                }
            }
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        public static void Register(string name, IMessageProducer pro)
        {
            Dictionary.TryAdd(name, pro);
        }

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="name">服务名称</param>
        /// <returns>传输对象构造器</returns>
        public static IMessageProducer GetService(string name)
        {
            if (Dictionary.TryGetValue(name, out var producer))
                return producer;
            return Default ??= IocHelper.Create<IMessageProducer>();
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static IApiResult Producer(string topic, string title, string content)
        {
            return GetService(topic).Producer(topic,  title,  content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static IApiResult Producer<T>(string topic, string title, T content)
        {
            return GetService(topic).Producer(topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static Task<IApiResult> ProducerAsync(string topic, string title, string content)
        {
            return GetService(topic).ProducerAsync(topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static Task<IApiResult> ProducerAsync<T>(string topic, string title, T content)
        {
            return GetService(topic).ProducerAsync(topic, title, content);
        }
    }
}
