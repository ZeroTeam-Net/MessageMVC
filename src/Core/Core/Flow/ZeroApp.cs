﻿using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.AddIn;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 应用扩展方法
    /// </summary>
    public static class ZeroApp
    {
        static int isInitialized = 0;

        /// <summary>
        /// 检查并注入配置
        /// </summary>
        /// <param name="services"></param>
        public static void AddDependency(this IServiceCollection services)
        {
            if (DependencyHelper.ServiceCollection != services)
            {
                DependencyHelper.SetServiceCollection(services);
            }

            //IZeroContext构造
            services.TryAddScoped<IZeroContext, ZeroContext>();
            services.TryAddTransient<IUser, UserInfo>();
            //序列化工具
            services.TryAddTransient<ISerializeProxy, NewtonJsonSerializeProxy>();
            services.TryAddTransient<IJsonSerializeProxy, NewtonJsonSerializeProxy>();
            services.TryAddTransient<IXmlSerializeProxy, XmlSerializeProxy>();

            //配置\依赖对象初始化,系统配置获取
            services.AddTransient<IFlowMiddleware, ConfigMiddleware>();
            //消息选择器
            services.AddTransient<IFlowMiddleware, MessagePoster>();
            //API路由与执行
            services.AddTransient<IMessageMiddleware, ApiExecuter>();
            //消息接收服务自动发现
            services.AddTransient<IReceiverDiscover, ReceiverDiscover>();
            //插件载入
            if (ZeroAppOption.Instance.EnableAddIn)
            {
                services.AddSingleton<IFlowMiddleware>(AddInImporter.Instance);
            }

            services.TryAddSingleton<IInlineMessage, InlineMessage>();

        }

        /// <summary>
        /// 使用System.Text.Json序列化工具
        /// </summary>
        /// <param name="services"></param>
        public static void UseMsJson(this IServiceCollection services)
        {
            services.TryAddTransient<ISerializeProxy, JsonSerializeProxy>();
            services.TryAddTransient<IJsonSerializeProxy, JsonSerializeProxy>();
            services.TryAddTransient<IXmlSerializeProxy, XmlSerializeProxy>();
        }

        /// <summary>
        /// 启动主流程控制器
        /// </summary>
        /// <param name="services">依赖服务</param>
        /// <param name="autoDiscover">对接口自动发现</param>
        public static async void UseFlow(this IServiceCollection services, bool autoDiscover = true)
        {
            if (Interlocked.Increment(ref isInitialized) == 1)
            {
                AddDependency(services);
                ZeroFlowControl.Check();
                if (autoDiscover)
                    ZeroFlowControl.Discove();
            }
            ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
        }

        /// <summary>
        /// 启动主流程控制器，发现指定类型所在的程序集的Api，由参数决定是否等待系统退出
        /// </summary>
        /// <param name="services"></param>
        /// <param name="type">需要发现服务的程序集的类型之一</param>
        public static void UseFlow(this IServiceCollection services, Type type)
        {
            UseFlow(services, type.Assembly);
        }

        /// <summary>
        /// 启动主流程控制器，发现指定程序集的Api，由参数决定是否等待系统退出
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">需要发现服务的程序集</param>
        public static async void UseFlow(this IServiceCollection services, Assembly assembly)
        {
            if (Interlocked.Increment(ref isInitialized) == 1)
            {
                AddDependency(services);
                ZeroFlowControl.Check();
                ZeroFlowControl.Discove(assembly);
            }
            ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
        }

        /// <summary>
        /// 启动主流程控制器，发现指定程序集的Api，适用于测试的场景
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">需要发现服务的程序集</param>
        public static void UseTest(this IServiceCollection services, Assembly assembly = null)
        {
            if (Interlocked.Increment(ref isInitialized) == 1)
            {
                services.TryAddTransient<IServiceReceiver, EmptyReceiver>();
                services.TryAddTransient<IMessageConsumer, EmptyReceiver>();
                services.TryAddTransient<INetEvent, EmptyReceiver>();
                AddDependency(services);
                ZeroFlowControl.Check();
                if (assembly != null)
                    ZeroFlowControl.Discove(assembly);
            }
            ZeroFlowControl.Initialize();
            ZeroFlowControl.Run();
        }

        /// <summary>
        /// 启动主流程控制器并等待退出
        /// </summary>
        /// <param name="services">依赖服务</param>
        /// <param name="autoDiscover">对接口自动发现</param>
        public static async Task UseFlowAndWait(this IServiceCollection services, bool autoDiscover = true)
        {
            if (Interlocked.Increment(ref isInitialized) == 1)
            {
                AddDependency(services);
                ZeroFlowControl.Check();
                if (autoDiscover)
                    ZeroFlowControl.Discove();
            }
            ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
            await ZeroFlowControl.WaitEnd();
        }

        /// <summary>
        /// 启动主流程控制器，发现指定类型所在的程序集的Api，由参数决定是否等待系统退出
        /// </summary>
        /// <param name="services"></param>
        /// <param name="type">需要发现服务的程序集的类型之一</param>
        public static Task UseFlowAndWait(this IServiceCollection services, Type type)
        {
            return UseFlowAndWait(services, type.Assembly);
        }

        /// <summary>
        /// 启动主流程控制器，发现指定程序集的Api，由参数决定是否等待系统退出
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly">需要发现服务的程序集</param>
        public static async Task UseFlowAndWait(this IServiceCollection services, Assembly assembly)
        {
            if (Interlocked.Increment(ref isInitialized) == 1)
            {
                AddDependency(services);
                ZeroFlowControl.Check();
                ZeroFlowControl.Discove(assembly);
            }
            ZeroFlowControl.Initialize();
            await ZeroFlowControl.RunAsync();
            await ZeroFlowControl.WaitEnd();
        }
    }
}