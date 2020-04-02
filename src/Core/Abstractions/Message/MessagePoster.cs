using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息投递
    /// </summary>
    public class MessagePoster : IFlowMiddleware
    {
        #region IFlowMiddleware

        string IZeroMiddleware.Name => "MessagePoster";

        int IZeroMiddleware.Level => int.MaxValue;

        #endregion

        #region 消息生产者

        /// <summary>
        /// 默认的生产者
        /// </summary>
        static IMessagePoster Default;

        private static readonly Dictionary<string, IMessagePoster> ServiceMap = new Dictionary<string, IMessagePoster>(StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, IMessagePoster> posters = new Dictionary<string, IMessagePoster>();

        /// <summary>
        ///     初始化
        /// </summary>
        void IFlowMiddleware.Initialize()
        {
            posters = IocHelper.RootProvider.GetServices<IMessagePoster>().ToDictionary(p => p.GetTypeName());
            var sec = ConfigurationManager.Get("MessagePoster");
            posters.TryGetValue(sec.GetStr("default", ""), out Default);
            foreach (var pro in posters)
            {
                pro.Value.Initialize();

                var strs = sec.GetStr(pro.Key, "")?.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in strs)
                {
                    ServiceMap.TryAdd(str, pro.Value);
                }
            }
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        public static void RegistPoster<TMessagePoster>(params string[] services)
            where TMessagePoster : IMessagePoster, new()
        {
            var name = typeof(TMessagePoster).GetTypeName();
            if (!posters.TryGetValue(name, out var poster))
            {
                posters.Add(name, poster = new TMessagePoster());
                poster.Initialize();
            }
            foreach (var service in services)
                ServiceMap.TryAdd(service, poster);
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        public static void RegistPoster( IMessagePoster poster, params string[] services)
        {
            foreach (var service in services)
                ServiceMap.TryAdd(service, poster);
        }

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="name">服务名称</param>
        /// <returns>传输对象构造器</returns>
        public static IMessagePoster GetService(string name)
        {
            return ServiceMap.TryGetValue(name, out var producer) 
                ? producer 
                : Default;
        }

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="item">消息</param>
        /// <returns>返回值</returns>
        public static Task<(MessageState state, string result)> Post(IMessageItem item)
        {
            var producer = GetService(item.Topic);
            if (producer == null)
                return Task.FromResult((MessageState.NoSupper, default(string)));
            return producer.Post(item);
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
        public static MessageState Publish<TArg>(string topic, string title, TArg content)
        {
            var (state, _) = Post(MessageItem.NewMessage(topic, title, content)).Result;
            return state;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static MessageState Publish(string topic, string title, string content)
        {
            var (state, _) = Post(MessageItem.NewMessage(topic, title, content)).Result;
            return state;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static async Task<MessageState> PublishAsync<TArg>(string topic, string title, TArg content)
        {
            var (state, _) = await Post(MessageItem.NewMessage(topic, title, content));
            return state;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static async Task<MessageState> PublishAsync(string topic, string title, string content)
        {
            var (state, _) = await Post(MessageItem.NewMessage(topic, title, content));
            return state;
        }

        #endregion

        #region 远程调用

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static TRes Call<TArg, TRes>(string service, string api, TArg args)
        {
            var (state, result) = Post(MessageItem.NewMessage(service, api, args)).Result;
            return state != MessageState.Success
                ? default
                : JsonHelper.TryDeserializeObject<TRes>(result);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static void Call<TArg>(string service, string api, TArg args)
        {
            Post(MessageItem.NewMessage(service, api, args)).Wait();
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <returns></returns>
        public static TRes Call<TRes>(string service, string api)
        {
            var (state, result) = Post(MessageItem.NewMessage(service, api)).Result;
            return state != MessageState.Success
                ? default
                : JsonHelper.TryDeserializeObject<TRes>(result);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static string Call(string service, string api, string args)
        {
            var (_, result) = Post(MessageItem.NewMessage(service, api, args)).Result;
            return result;
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static async Task<TRes> CallAsync<TArg, TRes>(string service, string api, TArg args)
        {
            var (state, result) = await Post(MessageItem.NewMessage(service, api, args));
            return state != MessageState.Success
                ? default
                : JsonHelper.TryDeserializeObject<TRes>(result);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static Task CallAsync<TArg>(string service, string api, TArg args)
        {
            return Post(MessageItem.NewMessage(service, api, args));
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <returns></returns>
        public static async Task<TRes> CallAsync<TRes>(string service, string api)
        {
            var (state, result) = await Post(MessageItem.NewMessage(service, api));
            return state != MessageState.Success
                ? default
                : JsonHelper.TryDeserializeObject<TRes>(result);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static async Task<string> CallAsync(string service, string api, string args)
        {
            var (_, result) = await Post(MessageItem.NewMessage(service, api, args));
            return result;
        }

        #endregion
    }
}
