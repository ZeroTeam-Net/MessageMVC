﻿using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息生产对象
    /// </summary>
    public class MessageProducer : IFlowMiddleware
    {
        #region IFlowMiddleware

        /// <summary>
        /// 默认的生产者
        /// </summary>
        public static IMessageProducer Default { get; set; }

        string IFlowMiddleware.RealName => "MessageProducer";

        int IFlowMiddleware.Level => int.MaxValue;

        #endregion
        #region 消息生产者

        private static readonly Dictionary<string, IMessageProducer> Producers = new Dictionary<string, IMessageProducer>(StringComparer.OrdinalIgnoreCase);

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
                    Producers.TryAdd(str, pro);
                }
                pro.Initialize();
            }
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        public static void Register(string name, IMessageProducer pro)
        {
            Producers.TryAdd(name, pro);
        }

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="name">服务名称</param>
        /// <returns>传输对象构造器</returns>
        public static IMessageProducer GetService(string name)
        {
            if (Producers.TryGetValue(name, out var producer))
            {
                return producer;
            }

            return Default ??= IocHelper.Create<IMessageProducer>();
        }
        #endregion

        #region 消息生产

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static TRes Producer<TArg, TRes>(string topic, string title, TArg content)
        {
            return GetService(topic).Producer<TArg, TRes>(topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static void Producer<TArg>(string topic, string title, TArg content)
        {
            GetService(topic).Producer(topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <returns></returns>
        public static TRes Producer<TRes>(string topic, string title)
        {
            return GetService(topic).Producer<TRes>(topic, title);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static string Producer(string topic, string title, string content)
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
        public static Task<TRes> ProducerAsync<TArg, TRes>(string topic, string title, TArg content)
        {
            return GetService(topic).ProducerAsync<TArg, TRes>(topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static Task ProducerAsync<TArg>(string topic, string title, TArg content)
        {
            return GetService(topic).ProducerAsync<TArg>(topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <returns></returns>
        public static Task<TRes> ProducerAsync<TRes>(string topic, string title)
        {
            return GetService(topic).ProducerAsync<TRes>(topic, title);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static Task<string> ProducerAsync(string topic, string title, string content)
        {
            return GetService(topic).ProducerAsync(topic, title, content);
        }
        #endregion


        #region 计划投送

        /// <summary>
        /// 计划生产者
        /// </summary>
        private static IPlanProducer PlanProducer;
        private static ZeroAppOption appOption;

        private static ZeroAppOption AppOption => appOption ??= IocHelper.Create<ZeroAppOption>();

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <returns>传输对象构造器</returns>
        private static IPlanProducer PlanService()
        {
            return PlanProducer ??= IocHelper.Create<IPlanProducer>() ?? new PlanProducer();
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static ApiResult Plan<TArg>(PlanOption option, string topic, string title, TArg content)
        {
            if (AppOption.EnableGlobalContext)
            {
                option.request_id = GlobalContext.Current.Request.RequestId;
                option.caller = AppOption.AppName;
                option.service = AppOption.ServiceName;
            }
            return PlanService().Plan(option, topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static ApiResult Plan(PlanOption option, string topic, string title, string content)
        {
            if (AppOption.EnableGlobalContext)
            {
                option.request_id = GlobalContext.Current.Request.RequestId;
                option.caller = AppOption.AppName;
                option.service = AppOption.ServiceName;
            }
            return PlanService().Plan(option, topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static Task<ApiResult> PlanAsync<TArg>(PlanOption option, string topic, string title, TArg content)
        {
            if (AppOption.EnableGlobalContext)
            {
                option.request_id = GlobalContext.Current.Request.RequestId;
                option.caller = AppOption.AppName;
                option.service = AppOption.ServiceName;
            }
            return PlanService().PlanAsync(option, topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static Task<ApiResult> PlanAsync(PlanOption option, string topic, string title, string content)
        {
            if (AppOption.EnableGlobalContext)
            {
                option.request_id = GlobalContext.Current.Request.RequestId;
                option.caller = AppOption.AppName;
                option.service = AppOption.ServiceName;
            }
            return PlanService().PlanAsync(option, topic, title, content);
        }

        #endregion
    }
}
